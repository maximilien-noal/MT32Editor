Feature: Patch Editor
    The patch editor allows configuring MT-32 patches with timbre assignments,
    key shift, fine tune, bender range, assign mode and reverb settings.

    Scenario: Create a new patch
        Given a new patch is created for slot 0
        Then the patch should have valid default values

    Scenario: Set timbre group and number
        Given a new patch is created for slot 0
        When I set the timbre group to 2
        And I set the timbre number to 32
        Then the timbre group should be 2
        And the timbre group type should be "Memory"
        And the timbre number should be 32

    Scenario Outline: Set patch parameters
        Given a new patch is created for slot 0
        When I set key shift to <keyshift>
        And I set fine tune to <finetune>
        And I set bender range to <bender>
        Then key shift should be <keyshift>
        And fine tune should be <finetune>
        And bender range should be <bender>

        Examples:
            | keyshift | finetune | bender |
            | 0        | 0        | 0      |
            | 12       | 25       | 12     |
            | 24       | -50      | 24     |

    Scenario: Set assign mode
        Given a new patch is created for slot 0
        When I set assign mode to 2
        Then assign mode should be 2

    Scenario: Toggle reverb
        Given a new patch is created for slot 0
        When I enable patch reverb
        Then patch reverb should be enabled
        When I disable patch reverb
        Then patch reverb should be disabled
