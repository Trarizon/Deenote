# Mouse FSM

``` mermaid
stateDiagram
    [*] --> Single : Shift Up
    state Single {
        [*] --> IdleSingle
        IdleSingle --> PlacingSingle : R Down
    }
    [*] --> Slides : Slides
    state Slides {
        [*] --> IdleSlides
        IdleSlides --> PlacingSlides : R Down
    }
    [*] --> Paste : Pasting
    state Paste {
        [*] --> IdlePaste
    }
    Placed --> [*]
    PlacingSingle --> Placed : R Up
    PlacingSlides --> Placed : R Up
    IdlePaste --> Placed : R Up

    PlacingSingle --> Aborted : L Down
    PlacingSlides --> Aborted : L Down
    PlacingPaste --> Aborted : R Down
    Aborted --> [*] : L Up & R Up

    IdleSingle --> Select : L Down
    IdleSlides --> Select : L Down
    IdlePaste --> Select : L Down

    state Select {
        [*] --> Selecting
        Selecting --> Selected : L Up | R Down
        Selected --> [*]
    }
    Select --> [*]

    state Warning {
        [*] --> IdleWarning
        IdleWarning --> PlacingWarning : R Down
        PlacingWarning --> PlacedWarning : R Up
    }
    PlacingWarning --> Aborted : L Down
```