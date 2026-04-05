Feature: Timbre Editor
    The timbre editor allows creating, editing and managing MT-32 timbres.
    Both WinForms and Avalonia UIs must provide the same editing capabilities.

    Scenario: Create a new audible timbre
        Given a new timbre editor is opened
        When I create a new audible timbre
        Then the timbre should have a valid name
        And the timbre should have 4 partials

    Scenario: Set timbre name
        Given a new timbre editor is opened
        When I set the timbre name to "TestBass"
        Then the timbre name should start with "TestBass"

    Scenario: Change active partial
        Given a new timbre editor is opened
        When I select partial 2
        Then the active partial should be 2

    Scenario: Toggle partial mute status
        Given a new timbre editor is opened
        When I mute partial 1
        Then partial 1 should be muted
        When I unmute partial 1
        Then partial 1 should not be muted

    Scenario: Set partial structure
        Given a new timbre editor is opened
        When I set structure 1-2 to 6
        Then structure 1-2 should be 6

    Scenario: Toggle sustain
        Given a new timbre editor is opened
        When I enable sustain
        Then sustain should be enabled
        When I disable sustain
        Then sustain should be disabled

    Scenario: Toggle pitch bend
        Given a new timbre editor is opened
        When I enable pitch bend on partial 0
        Then pitch bend on partial 0 should be enabled
        When I disable pitch bend on partial 0
        Then pitch bend on partial 0 should be disabled

    Scenario: Undo and redo timbre changes
        Given a new timbre editor is opened with history
        When I set the timbre name to "Changed"
        And I record the change in history
        Then the timbre name should start with "Changed"
        When I undo the last change
        Then the timbre should be restored to original state
        When I redo the last change
        Then the timbre name should start with "Changed"

    Scenario: Copy and paste partial
        Given a new timbre editor is opened
        When I set a parameter on partial 0
        And I copy partial 0
        And I paste to partial 1
        Then partial 1 should have the same parameter as partial 0

    Scenario Outline: Set pitch parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 0       | 48    |
            | 1       | 75    |
            | 2       | 8     |

    Scenario Outline: Set LFO parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 20      | 50    |
            | 21      | 75    |
            | 22      | 30    |

    Scenario Outline: Set TVF parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 23      | 80    |
            | 24      | 15    |
            | 25      | 10    |

    Scenario Outline: Set TVA parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 41      | 85    |
            | 42      | 60    |
            | 43      | 64    |

    Scenario: Set waveform type
        Given a new timbre editor is opened
        When I set waveform on partial 0 to 1
        Then waveform on partial 0 should be 1

    Scenario: All 58 partial parameters have valid defaults
        Given a new timbre editor is opened
        Then all 58 parameters for partial 0 should have valid values
