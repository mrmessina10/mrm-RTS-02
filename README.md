# Prototype: Real-Time Strategy (RTS)

![RTS_Camera_n_Movement](https://github.com/user-attachments/assets/d7077f00-0694-484e-b57b-40785900498c)
![RTS_selection](https://github.com/user-attachments/assets/c567d154-3b5e-40b9-82e6-9e2e74fabcf6)
![RTS_Patrol_n_Attack](https://github.com/user-attachments/assets/386661b5-f43b-4c85-8ac7-f57b986ee110)


## Descripción
Este es un prototipo en desarrollo de un juego de Estrategia en Tiempo Real (RTS) creado en Unity. El objetivo principal de este proyecto es explorar y construir sistemas arquitectónicos sólidos para el control de múltiples unidades, enfocándome principalmente en la implementación de lógicas de estado y sistemas de combate escalables.

## Arquitectura Técnica y Sistemas Core

El foco de este desarrollo está en escribir código modular y mantenible. Los sistemas principales incluyen:

*   **Máquina de Estados Finitos (FSM) para Unidades:** 
    Cada unidad opera bajo un sistema de estados (Idle, Move, Attack, Die). Esto permite desacoplar los comportamientos, haciendo que el agregado de nuevas acciones sea escalable y previniendo que la lógica se mezcle en funciones monolíticas.
*   **Sistema de Combate Diferenciado:**
    Implementación de lógicas distintas para unidades de combate *Melee* (cuerpo a cuerpo) y *Ranged* (a distancia). El sistema gestiona rangos de visión, detección de objetivos, tiempos de ataque (cooldowns) y eventos de animación para sincronizar el daño.
*   **Selección y Movimiento:**
    Lógica para la selección individual o múltiple de unidades y su desplazamiento utilizando el sistema de navegación (NavMesh) de Unity.

## Estructura del Proyecto

Todo el código fuente y la lógica desarrollada para este prototipo se encuentra centralizada y organizada en la carpeta principal:
*   `Assets/_Scripts/`: Contiene todos los scripts en C#, divididos por responsabilidad.

## Tecnologías Utilizadas
*   **Motor:** Unity 6.3 LTS (6000.3.9f1)
*   **Lenguaje:** C#

## Estado Actual y Próximos Pasos
El proyecto se encuentra en etapa de desarrollo activo. Los próximos hitos técnicos y de diseño incluyen:
- Sistema de Economía y Recursos: Desarrollo de la arquitectura subyacente para la recolección, almacenamiento y consumo de recursos, estableciendo el *game loop* central del RTS.
- Refactorización y optimización del sistema de búsqueda de objetivos (Targeting).
- Explorar la migración progresiva de estos conceptos arquitectónicos hacia Unreal Engine.
