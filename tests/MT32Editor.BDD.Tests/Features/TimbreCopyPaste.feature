Feature: Timbre Copy Paste
    The timbre editor should support copying and pasting partial parameters
    between partials, enabling efficient sound design.

    Scenario: Copy partial parameters and paste to another partial
        Given a timbre with modified partial 0
        When I copy partial 0 parameters
        And I paste partial 0 parameters to partial 2
        Then partial 2 should match partial 0 parameters

    Scenario: Paste is disabled until copy is performed
        Given a new timbre editor session
        Then paste should be unavailable
        When I copy partial 0 parameters
        Then paste should be available
