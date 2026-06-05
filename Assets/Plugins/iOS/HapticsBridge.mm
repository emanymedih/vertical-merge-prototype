#import <UIKit/UIKit.h>

extern "C"
{
    void VM_HapticLightImpact()
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [generator prepare];
            [generator impactOccurred];
        }
    }

    void VM_HapticHeavyImpact()
    {
        if (@available(iOS 10.0, *))
        {
            UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            [generator prepare];
            [generator impactOccurred];
        }
    }

    void VM_HapticSuccessPattern()
    {
        if (@available(iOS 10.0, *))
        {
            UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
        }
    }
}
