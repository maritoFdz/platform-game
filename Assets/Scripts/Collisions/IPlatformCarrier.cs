using System.Collections.Generic;
using UnityEngine;

public interface IPlatformCarrier
{
    public LayerMask PassengerMask { get; }
    public string OriginalTag { get; }

    public void UpdateMovingPlatformTag(CollisionsHandler2D controller)
    {
        if (controller.colDetails.onMovingPlatform)
        {
            if (!controller.gameObject.CompareTag("MovingPlatformCarrier"))
                controller.gameObject.tag = "MovingPlatformCarrier";
        }
        else if (controller.gameObject.CompareTag("MovingPlatformCarrier"))
            controller.gameObject.tag = OriginalTag;
    }

    public List<Transform> GetPassengersOnTop();
}
