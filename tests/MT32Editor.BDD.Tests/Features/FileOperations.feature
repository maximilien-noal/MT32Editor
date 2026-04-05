Feature: File Operations
    File operations handle saving and loading of timbre files, SysEx files,
    and configuration. Both WinForms and Avalonia must handle files identically.

    Scenario: Title bar shows application name
        Given a new MT-32 state is initialized for file operations
        Then the title bar text should contain "MT-32 Editor"

    Scenario: Title bar shows loaded filename
        Given a new MT-32 state is initialized for file operations
        When I set the title bar filename to "mysound.syx"
        Then the title bar text should contain "mysound.syx"

    Scenario: Title bar shows description from message
        Given a new MT-32 state is initialized for file operations
        When I set the system message to "My MT-32 Setup"
        Then the title bar text should contain "My MT-32 Setup"

    Scenario: FileTools validates SysEx file extension
        Then ".syx" should be a valid SysEx extension
        And ".mid" should be a valid MIDI extension
        And ".txt" should not be a valid SysEx or MIDI extension

    Scenario: ParseTools version extraction
        Then ParseTools should extract a valid version string

    Scenario: LogicTools boolean conversion roundtrip
        Then converting true to int should give 1
        And converting false to int should give 0
        And converting 1 to bool should give true
        And converting 0 to bool should give false

    Scenario: ParseTools string padding
        When I pad "AB" to 10 characters
        Then the padded string should be exactly 10 characters long

    Scenario: Config file persistence
        Given a fresh config state
        When I set dark mode to true
        Then dark mode should be true
        When I set dark mode to false
        Then dark mode should be false
