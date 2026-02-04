# ICH-X K3 Digital Cluster Prototype (Unity URP)

A digital instrument cluster prototype for the ICH-X K3 off-road SUV created using **Unity** and **URP**.  
It is a **modular** project, where data is sent to widgets using **EventBus**, and the UI is loaded dynamically using **Addressables**.

---

## Key Features

- Tachometer (RPM), speedometer (km/h), and RPM threshold warning
- PRND gear selector and "drive gear" logic (gear 1..n in D with upshift/downshift)
- Road, Trail, and Snow modes, configurable using ScriptableObject
- Day/Night theme, where UI components will use a color scheme based on the theme
- Off-road telemetry, where Pitch, Roll, and G-meter will be displayed
- Four-wheel tire pressure simulation, where the UI will update dynamically using EventBus

---

## Architectural Overview

### Runtime Flow
1. Bootstrap: The application is launched, and the kernel is initialized.
2. The kernel: Services are registered, such as the EventBus, VehicleDataService, ThemeService, and so on.
3. Addressables is initialized.
4. UiRoot and UI panels are dynamically created.
5. The simulation loop: Vehicle data is updated, and events are published
6. The UI panels listen to the events and update the graphics and text accordingly.

### Key Patterns / Concepts
- **Service Registry/Locator**: Services can be used without hard-coded references to the scene.
- **Publish/Subscribe**: The simulation and UI are separated by the EventBus.
- **ScriptableObject**: Themes and modes can be set up without modifying the code.

---

## Controls

### Driving and Simulation
- Arrow Keys: Accelerate/Brake/Steer (Telemetry & Roll/G derived)
- P / R / N / D: Gear selection

### Modes and Theme
- F1 / F2 / F3: Road / Trail / Snow
- F4: Toggle Day/Night

### Tire Pressure
- B: Deflate tire (Random selection)
- V: Reinflate tire

### Speed Limit
- 1..6: Quickly set 30/50/70/90/110/130 km/h

### Turn Signals
- Z: Left Turn Signal
- X: Hazard Lights
- C: Right Turn Signal

---

## Project Structure

- `Assets/Scenes`: Minimal entry scene  
- `Assets/Application` + `Assets/Domain` + `Assets/Infrastructure`: Bootstrap, Services, EventBus  
- `Assets/Scripts/MyGameFeatures/ClusterFeature`

---

## Usage Notes

This project is designed as a highly extensible and easy-to-extend prototype:

- add new UI panels as Addressable prefabs  
- add new terrain modes as ScriptableObjects  
- add new data sources (real instead of simulated) without changing the UI with the help of the EventBus  

## Roadmap

- traction states 2H/4H/4L), vehicle warnings, engine temperatures  
- compass/altimeter displays and basic diagnostic information  
- integration with real telemetry (CAN or advanced mocks)  
```
