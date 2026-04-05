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
            | 1       | 40    |
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

    Scenario Outline: Set pitch envelope parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 6       | 80    |
            | 7       | 5     |
            | 8       | 5     |
            | 9       | 70    |
            | 10      | 3     |

    Scenario Outline: Set pitch envelope time and level parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 11      | 50    |
            | 12      | 60    |
            | 13      | 70    |
            | 14      | 80    |
            | 15      | 40    |
            | 16      | 30    |
            | 17      | 20    |
            | 18      | 45    |
            | 19      | 35    |

    Scenario Outline: Set TVF bias and keyfollow parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 26      | 64    |
            | 27      | 5     |
            | 28      | 75    |
            | 29      | 50    |
            | 30      | 3     |
            | 31      | 2     |

    Scenario Outline: Set TVF envelope time and level parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 32      | 90    |
            | 33      | 80    |
            | 34      | 70    |
            | 35      | 60    |
            | 36      | 50    |
            | 37      | 40    |
            | 38      | 30    |
            | 39      | 20    |
            | 40      | 55    |

    Scenario Outline: Set TVA bias and envelope parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 44      | -5    |
            | 45      | 64    |
            | 46      | -4    |
            | 47      | 3     |
            | 48      | 2     |

    Scenario Outline: Set TVA envelope time and level parameters
        Given a new timbre editor is opened
        When I set parameter <paramNo> on partial 0 to <value>
        Then parameter <paramNo> on partial 0 should be <value>

        Examples:
            | paramNo | value |
            | 49      | 90    |
            | 50      | 80    |
            | 51      | 70    |
            | 52      | 60    |
            | 53      | 50    |
            | 54      | 40    |
            | 55      | 30    |
            | 56      | 20    |
            | 57      | 55    |

    Scenario: Clone timbre preserves all parameters
        Given a new timbre editor is opened
        When I set the timbre name to "Original"
        And I clone the timbre
        Then the cloned timbre should have name starting with "Original"
        And all parameters across all partials should match the original

    Scenario: Timbre name is truncated to max 10 characters
        Given a new timbre editor is opened
        When I set the timbre name to "VeryLongTimbreName"
        Then the timbre name should start with "VeryLongTi"

    Scenario: Set structure 3-4
        Given a new timbre editor is opened
        When I set structure 3-4 to 8
        Then structure 3-4 should be 8

    Scenario: Set PCM sample parameter
        Given a new timbre editor is opened
        When I set parameter 5 on partial 0 to 42
        Then parameter 5 on partial 0 should be 42

    Scenario: Mute all partials
        Given a new timbre editor is opened
        When I mute partial 0
        And I mute partial 1
        And I mute partial 2
        And I mute partial 3
        Then all partials should be muted

    Scenario: PCM waveform disables TVF controls conceptually
        Given a new timbre editor is opened
        When I set waveform on partial 0 to 1
        Then the waveform on partial 0 should be PCM
        And TVF controls should be conceptually disabled for PCM

    Scenario: LA Synth waveform enables TVF controls
        Given a new timbre editor is opened
        When I set waveform on partial 0 to 0
        Then the waveform on partial 0 should be LA Synth
        And TVF controls should be enabled for LA Synth

    Scenario: Switching from PCM to LA Synth re-enables TVF
        Given a new timbre editor is opened
        When I set waveform on partial 0 to 1
        And I set waveform on partial 0 to 0
        Then TVF controls should be enabled for LA Synth

    Scenario: Timbre undo restores previous state
        Given a new timbre editor is opened with history
        When I set the timbre name to "BeforeUndo"
        And I record the change in history
        And I set the timbre name to "AfterChange"
        And I record the change in history
        And I undo the last change
        Then the timbre name should start with "BeforeUndo"

    Scenario: Multiple undo and redo operations
        Given a new timbre editor is opened with history
        When I set the timbre name to "State1"
        And I record the change in history
        And I set the timbre name to "State2"
        And I record the change in history
        And I undo the last change
        And I redo the last change
        Then the timbre name should start with "State2"
