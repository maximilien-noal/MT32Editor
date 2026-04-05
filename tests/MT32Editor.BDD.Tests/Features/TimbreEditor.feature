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
