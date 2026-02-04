# Prototipo Cluster Digitale ICH-X K3 (Unity URP)

Prototipo di quadro strumenti digitale per il SUV off-road **ICH-X K3** creato usando **Unity** e **URP**.  
È un progetto **modulare**, dove i dati vengono inviati ai widget tramite **EventBus** e la UI viene caricata dinamicamente usando **Addressables**.

---

## Funzionalità principali

- Contagiri (RPM), tachimetro (km/h) e avviso soglia RPM
- Selettore marcia PRND e logica "drive gear" (marcia 1..n in D con upshift/downshift)
- Modalità Road, Trail e Snow, configurabili usando ScriptableObject
- Tema Day/Night, dove i componenti UI useranno uno schema colori in base al tema
- Telemetria off-road, dove verranno mostrati Pitch, Roll e G-meter
- Simulazione pressione pneumatici su quattro ruote, dove la UI si aggiornerà dinamicamente usando EventBus

---

## Panoramica architetturale

### Flusso runtime
1. Bootstrap: L’applicazione viene avviata e il kernel viene inizializzato.
2. Il kernel: I servizi vengono registrati, come EventBus, VehicleDataService, ThemeService e così via.
3. Addressables viene inizializzato.
4. UiRoot e i pannelli UI vengono creati dinamicamente.
5. Il loop di simulazione: I dati dei veicoli vengono aggiornati e gli eventi vengono pubblicati
6. I pannelli UI ascoltano gli eventi e aggiornano grafica e testo di conseguenza.

### Pattern e idee chiave
- **Service Registry/Locator**: I servizi possono essere usati senza riferimenti hard-coded alla scena.
- **Publish/Subscribe**: La simulazione e la UI sono separate tramite EventBus.
- **ScriptableObject**: Il tema e le modalità possono essere configurati senza modificare il codice.

---

## Controlli

### Guida e simulazione
- Frecce direzionali: Accelera/Frena/Sterza (Telemetria & Roll/G derivati)
- P / R / N / D: Selezione marcia

### Modalità e tema
- F1 / F2 / F3: Road / Trail / Snow
- F4: Toggle Day/Night

### Pressione pneumatici
- B: Sgonfia pneumatico (selezione casuale)
- V: Rigonia pneumatico

### Limite velocità
- 1..6: Imposta rapidamente 30/50/70/90/110/130 km/h

### Frecce direzionali
- Z: Freccia sinistra
- X: Quattro frecce
- C: Freccia destra

---

## Struttura del progetto

- `Assets/Scenes`: Scena di ingresso minimale  
- `Assets/Application` + `Assets/Domain` + `Assets/Infrastructure`: Bootstrap, Servizi, EventBus  
- `Assets/Scripts/MyGameFeatures/ClusterFeature`

---

## Note di utilizzo

Questo progetto è progettato come un prototipo altamente estendibile e facile da estendere:

- aggiungere nuovi pannelli UI come prefab Addressable  
- aggiungere nuove modalità terreno come ScriptableObject  
- aggiungere nuove sorgenti dati (reali invece che simulate) senza cambiare la UI con l’aiuto dell’EventBus  

---

## Roadmap

- stati trazione 2H/4H/4L), avvisi veicolo, temperature motore  
- display bussola/altimetro e informazioni diagnostiche di base  
- integrazione con telemetria reale (CAN o mock avanzati)  
