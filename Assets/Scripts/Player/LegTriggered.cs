using System.Collections.Generic;
using UnityEngine;

public class LegTriggered : MonoBehaviour
{
    enum BodyPart { Left, Right, Debug };
    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private MovementController movementController;
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (bodyPart == BodyPart.Left)
        {
            movementController.leftLegObstacleCounter += 1;
            playerController.leftFootController.legObstacleCounter += 1;
        }
        else if (bodyPart == BodyPart.Right)
        {
            movementController.rightLegObstacleCounter += 1;
            playerController.rightFootController.legObstacleCounter += 1;
        }

        if (bodyPart == BodyPart.Debug)
        {
            // Time.timeScale = 0f;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        // Only if the gameobject has one collider
        RaycastHit hit;
        Vector3 normal = Vector3.zero;
        Vector3 dir = contactPoint - transform.position;
        if(Physics.Raycast(transform.position, dir.normalized, out hit, dir.magnitude + 0.2f, playerController.obstacleMask)) // movementCntroller
        {
            if (hit.collider != other)
            {
                Debug.Log("Hit different collider");
                return;
            }
           
            contactPoint = hit.point;
            normal = hit.normal;
        }
        Vector3 closestFootColPointToObstacle = Physics.ClosestPoint(contactPoint - 2*normal, GetComponent<Collider>(), transform.position, transform.rotation);

        if (bodyPart == BodyPart.Left)
        {
            movementController.obstacleOnLeftLegContactPoint = contactPoint;
            movementController.closestLFootColPointToObstacle = closestFootColPointToObstacle;
            playerController.leftFootController.obstacleOnLegContactPoint = contactPoint;
            playerController.leftFootController.closestFootColPointToObstacle = closestFootColPointToObstacle;
        }
        else if (bodyPart == BodyPart.Right)
        {
            movementController.obstacleOnRightLegContactPoint = contactPoint;
            movementController.closestRFootColPointToObstacle = closestFootColPointToObstacle;
            playerController.rightFootController.obstacleOnLegContactPoint = contactPoint;
            playerController.rightFootController.closestFootColPointToObstacle = closestFootColPointToObstacle;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }
        if (bodyPart == BodyPart.Left)
        {
            movementController.leftLegObstacleCounter -= 1;
            playerController.leftFootController.legObstacleCounter -= 1;
        }
        else if (bodyPart == BodyPart.Right)
        {
            movementController.rightLegObstacleCounter -= 1;
            playerController.rightFootController.legObstacleCounter -= 1;
        }
    }

}
