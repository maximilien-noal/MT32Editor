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
