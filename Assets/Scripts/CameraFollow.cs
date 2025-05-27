using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
   public Transform target;
   Vector3 targetPos;
   public Vector3 offsetPos;
   public float moveSpeed = 6;
   public float smooth = 0.2f;
   private Vector3 velocity = Vector3.zero;

   void Start()
   {

   }

   void LateUpdate()
   {
      MoveWithTarget();
   }

   void MoveWithTarget()
   {
      targetPos = target.transform.position + offsetPos;
      transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smooth);
   }

    public void ToMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
