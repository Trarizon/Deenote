## Placing-Selection Coordinator FSM

``` mermaid
flowchart TD
    s([start]) --> Place
    Place --LMD--> Selection
```

### Placer FSM
``` mermaid
flowchart TD
    s([Entry]) --!SlidesRequested--> IdleSingle
    s --SlidesRequested--> IdleSlides
    s --PastedRequested--> IdlePaste

    subgraph Idle
        IdleSingle --SlidesRequested--> IdleSlides
        IdleSlides --!SlidesRequested--> IdleSingle
        IdleSingle --PasteRequested--> IdlePaste
    end

    Idle --LMDown--> Selecting
    Selecting --LMUp--> Selected
    Selected --> e

    subgraph Placing
        IdleSingle --RMDown--> PlacingSingle
        IdleSlides --RMDown--> PlacingSlides
    end

    PlacingSingle --RMUp--> Placed
    PlacingSlides --RMUp--> Placed
    IdlePaste --RMUp--> Placed

    Placed --> e
    Idle --LMD--> Cancel
    Placing --LMD--> Cancel
    Cancel --LRMU--> e

    e([end]) --> s
```