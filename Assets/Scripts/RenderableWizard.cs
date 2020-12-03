using UnityEngine;

public class RenderableWizard : MonoBehaviour {
  public enum State { Base, Moving }

  static float EaseInOut(float x) {
    return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
  }

  static float EaseOut(float x) {
    return 1 - Mathf.Pow(1 - x, 4);
  }

  [SerializeField] Animator Animator = null;
  [SerializeField] MeshRenderer MeshRenderer = null;

  [SerializeField] Material Team1TorchMaterial = null;
  [SerializeField] Material Team2TorchMaterial = null;
  [SerializeField] Color Team1TorchLightColor = default;
  [SerializeField] Color Team2TorchLightColor = default;
  [SerializeField] float TimeToTravel = default;


  State CurrentState;
  float RemainingTravelTime;

  Quaternion OriginalRotation;
  Quaternion DesiredRotation;

  Vector3 Heading;
  Vector3 Origin;
  Vector3 Destination;

  public void SetNewPath(in Vector3 destination) {
    Origin = transform.position;
    Destination = destination;
    Heading = Vector3.Normalize(Destination - Origin);
    OriginalRotation = transform.rotation;
    DesiredRotation = Quaternion.LookRotation(Heading, Vector3.up);
    RemainingTravelTime = TimeToTravel;
    CurrentState = State.Moving;
  }

  public void SetTeam(in int teamIndex) {
    MeshRenderer.material = teamIndex % 2 == 0 ? Team1TorchMaterial : Team2TorchMaterial;
  }

  public void Tick(in float dt) {
    switch (CurrentState) {
      case State.Moving: {
        if (RemainingTravelTime > 0) {
          var interpolant = 1 - RemainingTravelTime / TimeToTravel;
          var currentPosition = transform.position;
          var nextPosition = Vector3.Lerp(Origin, Destination, EaseInOut(interpolant));
          var distance = Vector3.Distance(nextPosition, currentPosition);
          var nextRotation = Quaternion.Slerp(OriginalRotation, DesiredRotation, EaseOut(interpolant));
          var forwardHeadingDotProduct = Vector3.Dot(transform.forward, Heading);
          var moveSpeed = distance / dt;

          transform.SetPositionAndRotation(nextPosition, nextRotation);
          Animator.SetFloat("ForwardHeadingDotProduct", forwardHeadingDotProduct);
          Animator.SetFloat("MoveSpeed", moveSpeed);
        } else {
          Animator.SetFloat("ForwardHeadingDotProduct", 0);
          Animator.SetFloat("MoveSpeed", 0);
          CurrentState = State.Base;
        }
        RemainingTravelTime = Mathf.Max(0, RemainingTravelTime - dt);
      }
      break;

      case State.Base: {

      }
      break;

      default:
      break;
    }
  }
}