# Prototype: Real-Time Strategy (RTS)

![RTS_Camera_n_Movement](https://github.com/user-attachments/assets/d7077f00-0694-484e-b57b-40785900498c)
![RTS_selection](https://github.com/user-attachments/assets/c567d154-3b5e-40b9-82e6-9e2e74fabcf6)
![RTS_Patrol_n_Attack](https://github.com/user-attachments/assets/386661b5-f43b-4c85-8ac7-f57b986ee110)


## Descripción
Prototipo en desarrollo de un juego de Estrategia en Tiempo Real (RTS) creado en Unity, en 3D sobre URP. El formato es **Tower Defense con componentes de Colony Sim**: el jugador arranca con una base chica al costado de un camino, recolecta recursos para levantar defensas y hacer crecer su pueblo, y recibe por ese mismo camino dos tipos de tráfico: **oleadas de enemigos** y **caravanas** que llegan a comerciar en paz.

El eje de tensión es la **Militarización**: una barra de dos polos (militar ↔ cultural) que pondera la producción, qué caravana aparece y qué bonificación recibe el jugador. Una ciudad muy militarizada defiende mejor pero comercia peor; una muy cultural comercia mejor pero llega débil a cada oleada.

El objetivo técnico del proyecto es explorar y construir sistemas arquitectónicos sólidos para el control de múltiples unidades, con foco en lógicas de estado, combate y economía escalables.

## Recursos del juego

*   **Recolectables — madera, comida y piedra:** los workers los cosechan de nodos del mapa y los llevan a un punto de depósito.
*   **Oro:** no se recolecta. Solo se consigue comerciando con las caravanas.
*   **Recursos estratégicos:** los producen talleres y edificios específicos, y son lo que piden las caravanas a cambio de su catálogo.

## Arquitectura Técnica y Sistemas Core

El foco de este desarrollo está en escribir código modular y mantenible. Los sistemas principales incluyen:

*   **Máquina de Estados Finitos (FSM) para Unidades:**
    Cada unidad opera bajo un sistema de estados (`IState` con `Enter/Tick/Exit`). Esto permite desacoplar los comportamientos: cada acción nueva entra como un estado nuevo y no como una rama dentro de uno existente.
*   **Contratos por interfaz:**
    Managers y estados operan contra `IHarvestable`, `IDropOffPoint`, `IConstructable`, `IDamageable` e `ISelectable`, nunca contra clases concretas.
*   **Sistema de Combate Diferenciado:**
    Lógicas distintas para unidades *Melee* y *Ranged*, con rangos de visión, detección de objetivos, cooldowns y eventos de animación para sincronizar el daño.
*   **Selección y Movimiento:**
    Click, shift-click, doble click por tipo, caja de selección, grupos de control 1-9 y movimiento en formación sobre NavMesh.
*   **Economía:**
    Workers con una FSM propia que cosechan, cargan y depositan en edificios de drop-off, con inventario y gasto centralizados en `ResourceManager`.
*   **Construcción:**
    Colocación de edificios con ghost, validación de terreno y costo; obra real levantada por workers; trazado de muros por tramos con vida independiente por segmento.
*   **Datos en ScriptableObjects:**
    Todo dato que no cambia en runtime (edificios, facciones, input, mapas) vive en un SO.
*   **Editor de mapas** (en desarrollo):
    Ventana de editor para esculpir y pintar terreno, colocar recursos, trazar el camino y marcar spawns y zonas edificables.

## Estructura del Proyecto

*   `Assets/_Scripts/`: todos los scripts en C#, divididos por responsabilidad.
*   `Docs/`: documentos de diseño del juego, roadmap del vertical slice y un `.md` por script en `Docs/Scripts/`.
*   [PROJECT_STATUS.md](PROJECT_STATUS.md): snapshot consolidado de qué está construido y qué falta.

## Tecnologías Utilizadas
*   **Motor:** Unity 6.5 (`6000.5.6f1`), Universal Render Pipeline 17.5.0
*   **Lenguaje:** C#
*   **Paquetes:** AI Navigation 2.0.14 (NavMesh), Cinemachine 3.1.7, Input System 1.20.0

## Estado Actual y Próximos Pasos
El proyecto se encuentra en etapa de desarrollo activo, rumbo a un vertical slice jugable de punta a punta. Detalle en [Docs/Roadmap-VerticalSlice.md](Docs/Roadmap-VerticalSlice.md).

Hecho:
- Cámara RTS, selección, movimiento y combate melee/ranged.
- Loop de economía: recolección, depósito y gasto de recursos.
- Construcción de edificios y muros con obra por workers.

En curso:
- Editor de mapas para armar el terreno y el camino.
- Diseño del HUD de partida, ya maquetado ([Docs/Design-HUD.md](Docs/Design-HUD.md)).

Próximo:
- Edificios funcionales: City Center, producción de unidades, límite poblacional y torre defensiva.
- Enemigos que avanzan por el camino y rondas de oleadas.
- Caravanas, recursos estratégicos e índice de Militarización.
- Condiciones de victoria y derrota, y el HUD dentro del juego.
- Explorar la migración progresiva de estos conceptos arquitectónicos hacia Unreal Engine.
