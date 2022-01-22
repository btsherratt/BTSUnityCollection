using UnityEngine;

public class QuickCollisionWorld {
    public static int QueryCollisions(QuickCollisionPair[] collisionPairsOut, QuickCollider collider, int collisionMask = 0xFF) {
        int collisionCount = 0;

        foreach (QuickCollider testCollider in QuickCollider.All(true)) {
            if (testCollider != collider && (testCollider.m_collisionMask & collisionMask) > 0) {
                if (TestCircleCircle((QuickCircleCollider)collider, (QuickCircleCollider)testCollider)) {
                    if (collisionPairsOut != null) {
                        collisionPairsOut[collisionCount] = new QuickCollisionPair(collider, testCollider);
                    }
                    ++collisionCount;
                }
            }
        }

        return collisionCount;
    }

    static bool TestCircleCircle(QuickCircleCollider colliderA, QuickCircleCollider colliderB) {
        Vector3 positionA = colliderA.transform.TransformPoint(colliderA.m_center);
        Vector3 positionB = colliderB.transform.TransformPoint(colliderB.m_center);
        Vector3 delta = positionB - positionA;
        float combinedRadii = colliderA.m_radius + colliderB.m_radius;
        return delta.sqrMagnitude <= (combinedRadii * combinedRadii);
    }
}
