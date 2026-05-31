# LLM Valley

A proof-of-concept life simulation game developed in Unity to investigate the integration of Large Language Models (LLMs) into real-time NPC interaction systems.

### Project Overview

LLM Valley is a senior design project developed within the scope of CMPE 492 / SENG 492. The project explores whether Large Language Models can be integrated into a real-time game environment to replace traditional static dialogue trees with dynamic and context-aware NPC conversations.

The prototype combines a lightweight life simulation environment with an AI-powered dialogue system. Players can interact with NPCs through free-text input while engaging with gameplay systems such as farming, trading, inventory management, and day progression. NPC responses are generated through an external LLM service and are influenced by character personas, relationship statistics, and conversation history.

### Project Website

https://llmvalley.vercel.app

The reports and user manual are available on the website

### Demonstration

#### Main Menu

<p align="center">
  <img src="https://github.com/user-attachments/assets/f80ee518-140b-4642-94df-945130c3dd32" width="700"/>
</p>

#### NPC Dialogue

<p align="center">
  <img src="https://github.com/user-attachments/assets/6cb9eb29-f104-4929-9b9c-aa61fecef353" width="450"/>
  <img src="https://github.com/user-attachments/assets/7b4ae9c1-bcf7-44a4-b970-2a35eebb5910" width="450"/>
</p>

#### Farming and Fishing System

<p align="center">
  <img src="https://github.com/user-attachments/assets/5e9cd4ea-9af2-4b63-8704-0a8fab2e83ef" width="450"/>
  <img src="https://github.com/user-attachments/assets/464c0bf4-46af-494c-8a17-36f2ea87dac4" width="450"/>
</p>



### Project Objectives

* Develop a working LLM-driven NPC dialogue system capable of processing natural language input.
* Replace traditional dialogue trees with dynamic and context-aware conversations.
* Integrate AI-generated dialogue into a playable life simulation environment.
* Maintain a modular architecture that separates gameplay systems from AI-provider-specific implementations.
* Evaluate the feasibility of using cloud-based LLM services within a real-time game application.

### System Architecture

The system is organized into three primary layers:

#### Game Client Layer

Responsible for:

* Player movement
* User interface management
* World interactions
* Farming and inventory systems
* Scene management

#### AI Integration Layer

Responsible for:

* Prompt generation
* NPC persona management
* Conversation history management
* OpenRouter communication
* Response processing

#### Simulation Layer

Responsible for:

* Relationship progression
* Calendar and day progression systems
* Trading and economy systems
* Save/load operations
* World-state management


### Main Interaction Flow

1. The player initiates an interaction with an NPC.
2. A free-text message is entered through the dialogue interface.
3. The system combines player input with NPC persona data and conversation history.
4. The generated prompt is sent to the selected language model through OpenRouter.
5. The model returns a context-aware response.
6. The response is displayed through the dialogue interface.
7. Relationship statistics are updated according to the conversation outcome.

### Implemented Features

#### AI-Powered NPC Dialogue

* Free-text NPC conversations
* Context-aware response generation
* Persistent conversation history
* Persona-driven dialogue behavior
* Relationship-aware interactions

#### Relationship System

Each NPC maintains relationship statistics including:

* Friendship
* Trust
* Love

These values influence future interactions and progression.

#### Gameplay Systems

* Farming and crop progression
* Inventory management
* Hotbar system
* Trading mechanics
* Calendar and day progression
* Scene transitions

#### Persistence System

The project implements local persistence using:

* PlayerPrefs
* JSON serialization

Stored data includes:

* Player inventory
* Calendar state
* Scene information
* NPC conversations
* Relationship statistics

### Technologies Used

| Category             | Technology       |
| -------------------- | ---------------- |
| Game Engine          | Unity 6          |
| Programming Language | C#               |
| AI Service           | OpenRouter API   |
| Language Model       | Claude Haiku 4.5 |
| Communication        | HTTPS            |
| Serialization        | JSON             |
| User Interface       | TextMeshPro      |
| Version Control      | GitHub           |

### Installation

#### Requirements

* Unity Hub
* Unity Editor 6000.3.8f1
* OpenRouter API Key

#### Clone Repository

```bash
git clone https://github.com/egemenasim/LLMValley.git
```

#### Open Project

1. Open Unity Hub.
2. Select **Add Project From Disk**.
3. Select the cloned repository.
4. Allow Unity to import all dependencies.

### Running the Project

1. Open:

```text
Assets/Scenes/Menu/MainMenu.unity
```

2. Enter Play Mode.
3. Open the API Key panel.
4. Enter a valid OpenRouter API key.
5. Start a new game.

### Current Limitations

The current implementation focuses on demonstrating the feasibility of LLM-supported NPC interaction and therefore includes several limitations:

* Dependence on external API availability
* Limited NPC memory scope
* Small-scale game world
* Prototype-level gameplay balancing
* Limited long-term dialogue persistence

### Future Work

Potential future improvements include:

* Long-term NPC memory systems
* Expanded world simulation
* Additional gameplay mechanics
* Localization support
* Advanced dialogue moderation
* Automated testing pipelines
* Support for multiple LLM providers

### Authors

* Yusuf Sadi Pesen
* Deniz Şeker
* Egemen Asım Ersoy
* Reyhan Sena Çimen

**Supervisor:** Dr. Eren Ulu

 ### License

This project was developed as an academic senior design project and is intended for educational and research purposes.
