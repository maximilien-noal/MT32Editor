Feature: System Settings
    The system settings editor allows configuring MT-32 master volume,
    tuning, reverb, MIDI channels and partial reserve.

    Scenario: Set master volume
        Given a new system level configuration
        When I set master volume to 80
        Then master volume should be 80

    Scenario: Set master tune
        Given a new system level configuration
        When I set master tune to 64
        Then master tune should be 64
        And master tune frequency should be displayed

    Scenario: Configure reverb
        Given a new system level configuration
        When I set reverb mode to 2
        And I set reverb time to 5
        And I set reverb level to 4
        Then reverb mode should be 2
        And reverb time should be 5
        And reverb level should be 4

    Scenario: Set MIDI channels 1 to 8
        Given a new system level configuration
        When I set MIDI channels 1 to 8
        Then MIDI channels should be configured as 1 to 8

    Scenario: Set MIDI channels 2 to 9
        Given a new system level configuration
        When I set MIDI channels 2 to 9
        Then MIDI channels should be configured as 2 to 9

    Scenario: Set partial reserve
        Given a new system level configuration
        When I set partial reserve for part 0 to 8
        Then partial reserve for part 0 should be 8
