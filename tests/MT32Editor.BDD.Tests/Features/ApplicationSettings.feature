Feature: Application Settings
    The application must persist toggle settings like autosave, dark mode,
    hardware connection, and system config options between sessions.

    Scenario: Toggle autosave setting
        Given the default application state
        When I toggle autosave off
        Then autosave should be disabled
        When I toggle autosave on
        Then autosave should be enabled

    Scenario: Toggle ignore system config on load
        Given the default application state
        When I enable ignore system config on load
        Then ignore system config on load should be enabled
        When I disable ignore system config on load
        Then ignore system config on load should be disabled

    Scenario: Toggle exclude system config on save
        Given the default application state
        When I enable exclude system config on save
        Then exclude system config on save should be enabled
        When I disable exclude system config on save
        Then exclude system config on save should be disabled

    Scenario: Toggle hardware MT-32 connected
        Given the default application state
        When I disconnect hardware MT-32
        Then hardware MT-32 should be disconnected
        When I connect hardware MT-32
        Then hardware MT-32 should be connected

    Scenario: Toggle send messages to MT-32 display
        Given the default application state
        When I disable send messages to MT-32
        Then send messages to MT-32 should be disabled
        When I enable send messages to MT-32
        Then send messages to MT-32 should be enabled

    Scenario: Toggle allow MT-32 reset
        Given the default application state
        When I enable allow MT-32 reset
        Then allow MT-32 reset should be enabled
        When I disable allow MT-32 reset
        Then allow MT-32 reset should be disabled
