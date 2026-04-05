Feature: Rhythm Editor
    The rhythm editor allows configuring MT-32 rhythm channel instruments.

    Scenario: Create a new rhythm bank entry
        Given a new rhythm bank for key 24
        Then the rhythm bank should have valid defaults

    Scenario: Set rhythm timbre
        Given a new rhythm bank for key 24
        When I set the rhythm timbre group to 0
        And I set the rhythm timbre number to 16
        Then the rhythm timbre group should be 0
        And the rhythm timbre number should be 16

    Scenario: Set rhythm pan pot
        Given a new rhythm bank for key 24
        When I set the rhythm pan pot to 7
        Then the rhythm pan pot should be 7

    Scenario: Set rhythm output level
        Given a new rhythm bank for key 24
        When I set the rhythm output level to 80
        Then the rhythm output level should be 80

    Scenario: Toggle rhythm reverb
        Given a new rhythm bank for key 24
        When I enable rhythm reverb
        Then rhythm reverb should be enabled
        When I disable rhythm reverb
        Then rhythm reverb should be disabled

    Scenario: Rhythm timbre group type validation
        Given a new rhythm bank for key 24
        When I set the rhythm timbre group to 0
        Then the rhythm timbre group type should be "Memory"
        When I set the rhythm timbre group to 1
        Then the rhythm timbre group type should be "Rhythm"

    Scenario: Pan pot boundary values
        Given a new rhythm bank for key 24
        When I set the rhythm pan pot to -7
        Then the rhythm pan pot should be -7
        When I set the rhythm pan pot to 0
        Then the rhythm pan pot should be 0

    Scenario: Output level boundary values
        Given a new rhythm bank for key 24
        When I set the rhythm output level to 0
        Then the rhythm output level should be 0
        When I set the rhythm output level to 100
        Then the rhythm output level should be 100
