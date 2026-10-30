# Game state and act flow

```mermaid
stateDiagram-v2
    [*] --> Intro
    Intro --> Dusk: StoryBeat A1-B01
    Dusk --> Night: StoryBeat A1-B06
    Night --> Storm: StoryBeat A3-B01
    Storm --> Finale: StoryBeat A4-B06
    Finale --> Dawn: StoryBeat A5-B04
    Dawn --> Ending: StoryBeat A5-B06
    Ending --> [*]
```

`GameStateMachine` owns these runtime states and transitions. When
`ActFlowController.enableSceneChaining` is true, the scene flow is:

```mermaid
flowchart LR
    Act1 -->|A1-B06| Act2
    Act2 -->|A2-B07| Act3
    Act3 -->|A3-B09| Act4
    Act4 -->|A4-B06| Act5
```

The full-game transition loads the next scene additively, makes it active,
unloads the previous scene, unloads unused assets, and restores `SaveSystem`
state. Direct act test scenes can start at configured later states.
