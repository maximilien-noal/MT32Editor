Feature: Memory Bank Editor
    The memory bank editor allows managing 64 custom timbres in MT-32 memory.

    Scenario: Initial memory bank has 64 slots
        Given a new MT-32 state is initialized
        Then the memory bank should have 64 timbre slots
        And each memory timbre should be accessible

    Scenario: Store and retrieve a custom timbre
        Given a new MT-32 state is initialized
        When I create a custom timbre named "MySynth"
        And I store it in memory slot 10
        Then memory slot 10 should contain the timbre
        And the timbre in slot 10 should be named "MySynth"

    Scenario: Select memory timbre
        Given a new MT-32 state is initialized
        When I select memory timbre 32
        Then the selected memory timbre should be 32

    Scenario: Copy and paste timbre between memory slots
        Given a new MT-32 state is initialized
        When I create a custom timbre named "CopyMe"
        And I store it in memory slot 5
        And I copy memory timbre from slot 5 to slot 20
        Then the timbre in slot 20 should be named "CopyMe"

    Scenario: Clear a memory timbre slot
        Given a new MT-32 state is initialized
        When I create a custom timbre named "ToBeCleared"
        And I store it in memory slot 3
        And I clear memory slot 3
        Then memory slot 3 should be empty

    Scenario: Memory bank stores timbre names correctly
        Given a new MT-32 state is initialized
        When I create a custom timbre named "TestPad"
        And I store it in memory slot 0
        Then the timbre names list should contain "TestPad" at memory position 0

    Scenario: Multiple timbres in different slots
        Given a new MT-32 state is initialized
        When I create a custom timbre named "Bass"
        And I store it in memory slot 0
        And I create a custom timbre named "Lead"
        And I store it in memory slot 1
        Then the timbre in slot 0 should be named "Bass"
        And the timbre in slot 1 should be named "Lead"

    Scenario: Selected memory timbre boundary values
        Given a new MT-32 state is initialized
        When I select memory timbre 0
        Then the selected memory timbre should be 0
        When I select memory timbre 63
        Then the selected memory timbre should be 63
