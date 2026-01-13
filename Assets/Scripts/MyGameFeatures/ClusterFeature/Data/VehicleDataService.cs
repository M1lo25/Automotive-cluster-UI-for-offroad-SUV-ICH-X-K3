//Servizio di simulazione dati veicolo che gestisce input:marce,gas,freno,sterzo,
//aggiorna Speed,RPM,Roll,G in modo semplificato e notifica gli altri sistemi tramite event bus

using UnityEngine;
using ICXK3.Domain;
using Gear = ICXK3.Gear;

namespace ICXK3
{
    public class VehicleDataService : IVehicleDataService
    {
        public float   SpeedKph  { get; private set; }
        public float   Rpm       { get; private set; }
        public float   RollDeg   { get; private set; }
        public Vector2 G         { get; private set; }
        public Gear    DriveGear { get; private set; } = Gear.P;

        private readonly IBroadcaster _bus;

        static readonly KeyCode GearPKey      = KeyCode.P;
        static readonly KeyCode GearNKey      = KeyCode.N;
        static readonly KeyCode GearDKey      = KeyCode.D;
        static readonly KeyCode GearRKey      = KeyCode.R;

        static readonly KeyCode ThrottleKey   = KeyCode.UpArrow;
        static readonly KeyCode BrakeKey      = KeyCode.DownArrow;
        static readonly KeyCode SteerLeftKey  = KeyCode.LeftArrow;
        static readonly KeyCode SteerRightKey = KeyCode.RightArrow;

        const float VMaxForward      = 185f;
        const float VMaxReverse      = 30f;

        const float ThrottleAccelFwd = 10f;
        const float ThrottleAccelRev = 8f;
        const float CoastDecelBase   = 1.5f;
        const float CoastDecelFactor = 0.015f;
        const float BrakeDecel       = 28.5f;

        const float IdleRpm           = 1700f;
        const float RpmMax            = 5500f;
        const float RpmBlipOnThrottle = 300f;
        const float RpmFollow         = 6f;

        const float FreeRevTargetRpm       = 5500f;
        const float FreeRevRisePerSec      = 5000f;
        const float FreeRevRisePerSecBoost = 13500f;
        const float FreeRevFallPerSec      = 4000f;

        const float GearLockKph       = 2.0f;

        const float G2MS2             = 9.81f;

        const float GravityMs2        = 9.81f;
        const float SlopeAccelFactor  = 0.35f;
        static readonly KeyCode PitchDownKey = KeyCode.L;

        const float HillHoldDeg = 1.0f;
        const float DrivelineDamping = 0.85f;
        const float ThrottleSlopeInfluence = 0.65f;

        const float StartupIdleRampTime = 0.5f;
        const float RpmFollowStartup    = 12f;
        const float RpmFollowPNThrottle = 18f;

        float _prevSpeedMs;
        float _steer;
        const float SteerAccel  = 2.0f;
        const float SteerReturn = 3.0f;
        const float YawRateMax  = 0.9f;

        float _rpmStartupT = 0f;
        bool  _rpmStartupActive = true;

        //Costruzione del servizio con dipendenza al bus per broadcast degli eventi runtime
        public VehicleDataService(IBroadcaster bus) { _bus = bus; }

        //Imposta la marcia con lock di sicurezza (es.niente P/R sopra una certa velocità)
        //e notifica il cambio marcia
        public void SetGear(Gear g)
        {
            if ((g == Gear.P || g == Gear.R) && SpeedKph > GearLockKph)
            {
                Debug.LogWarning($"[Vehicle] Cambio negato: richiesta {g} a {SpeedKph:F1} km/h (> {GearLockKph} km/h).");
                if (ServiceRegistry.TryResolve<IAudioService>(out var audio)) audio.Play("error", priority: 5);
                return;
            }

            if (DriveGear == g) return;
            DriveGear = g;

            if (DriveGear == Gear.P) SpeedKph = 0f;

            _bus.Broadcast(new OnGearChanged(DriveGear));
        }

        //Curva di accelerazione forward  per dare una progressione più credibile alle alte velocità
        float GetAccelFwdKphPerSec(float speedKph)
        {
            if (speedKph < 50f)   return 10.868f;
            if (speedKph < 90f)   return  9.090f;
            if (speedKph < 130f)  return  8.888f;
            return 10.000f;
        }

        //Tick di simulazione:legge input,integra velocità e RPM,stima G laterale e longitudinale e pubblica eventi
        public void SimTick(float dt)
        {
            if (Input.GetKeyDown(GearPKey)) SetGear(Gear.P);
            if (Input.GetKeyDown(GearNKey)) SetGear(Gear.N);
            if (Input.GetKeyDown(GearDKey)) SetGear(Gear.D);
            if (Input.GetKeyDown(GearRKey)) SetGear(Gear.R);

            bool throttle = Input.GetKey(ThrottleKey);
            bool brake    = Input.GetKey(BrakeKey);

            float coastDecel = Mathf.Max(CoastDecelBase, CoastDecelFactor * SpeedKph);

            //Dinamica velocità in D:accel con gas,coast senza gas e correzioni per pendenza(pitch)
            if (DriveGear == Gear.D)
            {
                if (throttle)
                {
                    float aKphPerSec = GetAccelFwdKphPerSec(SpeedKph);
                    float pitchDeg = InclinometerPitchController.CurrentPitchDeg;
                    float absDeg   = Mathf.Abs(pitchDeg);
                    if (absDeg > 0.1f)
                    {
                        float slopeRad = absDeg * Mathf.Deg2Rad;
                        float slopeKphPerSec = SlopeAccelFactor * GravityMs2 * Mathf.Sin(slopeRad) * 3.6f;
                        float signedSlope = (pitchDeg > 0f ? -slopeKphPerSec : slopeKphPerSec) * ThrottleSlopeInfluence;
                        aKphPerSec += signedSlope;
                    }
                    SpeedKph += aKphPerSec * dt;
                }
                else
                {
                    SpeedKph  = Mathf.Max(0f, SpeedKph - coastDecel * dt);

                    //Se in discesa,aggiunge "spinta" naturale dopo una soglia e con damping driveline
                    float pitchDownDeg = Mathf.Max(0f, -InclinometerPitchController.CurrentPitchDeg);
                    if (pitchDownDeg > 0.1f && !brake)
                    {
                        float effectiveDeg = Mathf.Max(0f, pitchDownDeg - HillHoldDeg);
                        if (effectiveDeg > 0f)
                        {
                            float slopeRad = effectiveDeg * Mathf.Deg2Rad;
                            float slopeAccelKphPerSec = SlopeAccelFactor * GravityMs2 * Mathf.Sin(slopeRad) * 3.6f;
                            SpeedKph += (slopeAccelKphPerSec * DrivelineDamping) * dt;
                        }
                    }

                    //Se in salita,aumenta la decelerazione
                    float pitchUpDeg = Mathf.Max(0f, InclinometerPitchController.CurrentPitchDeg);
                    if (pitchUpDeg > 0.1f && !brake)
                    {
                        float effectiveUpDeg = Mathf.Max(0f, pitchUpDeg - HillHoldDeg);
                        if (effectiveUpDeg > 0f)
                        {
                            float slopeRadUp = effectiveUpDeg * Mathf.Deg2Rad;
                            float slopeDecelKphPerSec = SlopeAccelFactor * GravityMs2 * Mathf.Sin(slopeRadUp) * 3.6f;
                            float extraDecel = (slopeDecelKphPerSec * DrivelineDamping) * dt;
                            SpeedKph = Mathf.Max(0f, SpeedKph - extraDecel);
                        }
                    }
                }
                if (brake)    SpeedKph -= BrakeDecel * dt;
            }
            //Dinamica in R:accel reverse,coast e freno
            else if (DriveGear == Gear.R)
            {
                if (throttle) SpeedKph += ThrottleAccelRev * dt;
                else          SpeedKph  = Mathf.Max(0f, SpeedKph - coastDecel * dt);
                if (brake)    SpeedKph -= BrakeDecel * dt;
            }
            //In P/N:solo coast più un eventuale "roll down" manuale in N e freno
            else
            {
                SpeedKph = Mathf.Max(0f, SpeedKph - coastDecel * dt);

                if (DriveGear == Gear.N)
                {
                    float pitchDownDeg = Mathf.Max(0f, -InclinometerPitchController.CurrentPitchDeg);
                    if (Input.GetKey(PitchDownKey) && pitchDownDeg > 0.1f)
                    {
                        float slopeRad = pitchDownDeg * Mathf.Deg2Rad;
                        float slopeAccelKphPerSec = SlopeAccelFactor * GravityMs2 * Mathf.Sin(slopeRad) * 3.6f;
                        SpeedKph += slopeAccelKphPerSec * dt;
                    }
                }

                if (brake)    SpeedKph -= BrakeDecel * dt;
            }

            //Clamp di velocità in base alla marcia
            float vMax = (DriveGear == Gear.R) ? VMaxReverse : VMaxForward;
            SpeedKph = Mathf.Clamp(SpeedKph, 0f, vMax);

            bool isDrive = (DriveGear == Gear.D || DriveGear == Gear.R);
            bool isPN    = !isDrive;

            //Calcolo RPM:in D/R segue la velocità, in P/N simula free-rev
            float targetRpm;
            if (isDrive)
            {
                float t = Mathf.Clamp01(SpeedKph / vMax);
                targetRpm = Mathf.Lerp(IdleRpm, RpmMax, t);
                if (throttle) targetRpm += RpmBlipOnThrottle;
                targetRpm = Mathf.Clamp(targetRpm, IdleRpm, RpmMax);
            }
            else
            {
                targetRpm = throttle
                    ? Mathf.MoveTowards(Rpm, FreeRevTargetRpm, FreeRevRisePerSecBoost * dt)
                    : Mathf.MoveTowards(Rpm, IdleRpm,         FreeRevFallPerSec * dt);
            }

            //Fase startup:tiene RPM a minimo per un breve ramp
            if (_rpmStartupActive)
            {
                targetRpm = IdleRpm;
                _rpmStartupT += dt;
                if (_rpmStartupT >= StartupIdleRampTime) _rpmStartupActive = false;
            }

            //Selezione degli RPM per risposta più pronta in startup e in P/N con gas
            float rpmFollowNow = RpmFollow;
            if (_rpmStartupActive) rpmFollowNow = Mathf.Max(rpmFollowNow, RpmFollowStartup);
            if (isPN && throttle)  rpmFollowNow = Mathf.Max(rpmFollowNow, RpmFollowPNThrottle);

            Rpm = Mathf.Lerp(Rpm, targetRpm, dt * rpmFollowNow);

            //Steering che integra input sinistra/destra con ritorno a centro e 
            //calcola yaw-rate semplificato
            bool steerLeft  = Input.GetKey(SteerLeftKey);
            bool steerRight = Input.GetKey(SteerRightKey);

            if (steerLeft)      _steer = Mathf.MoveTowards(_steer, -1f, SteerAccel * dt);
            else if (steerRight)_steer = Mathf.MoveTowards(_steer,  1f, SteerAccel * dt);
            else                _steer = Mathf.MoveTowards(_steer,   0f, SteerReturn * dt);

            float yawRate = _steer * YawRateMax;
            float egoMs   = SpeedKph / 3.6f;
            float latAcc  = egoMs * yawRate;
            float lonAcc  = (egoMs - _prevSpeedMs) / Mathf.Max(1e-4f, dt);

            //Vettore G:x laterale,y longitudinale,normalizzato a g
            G = new Vector2(latAcc / G2MS2, lonAcc / G2MS2);

            //Roll visivo derivato dalla G laterale con smoothing
            float targetRoll = Mathf.Atan2(G.x, 1f) * Mathf.Rad2Deg;
            RollDeg = Mathf.Lerp(RollDeg, targetRoll, dt * 5f);

            //Broadcast dei cambi,alcuni duplicati per compatibilità con listener diversi
            _bus.Broadcast(new OnSpeedChanged(SpeedKph));
            _bus.Broadcast(new SpeedChanged(SpeedKph));
            _bus.Broadcast(new OnRpmChanged(Rpm));
            _bus.Broadcast(new RpmChanged(Rpm));
            _bus.Broadcast(new OnRollChanged(RollDeg));
            _bus.Broadcast(new OnGChanged(G));

            _prevSpeedMs = egoMs;
        }
    }
}
