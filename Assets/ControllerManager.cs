using UnityEngine;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;
public class ControllerManager : MonoBehaviour {

    public GameObject Marker;
    public GameObject Follower;
    public GameObject RightController;

    private List<GameObject> points = new List<GameObject>();
    private GameObject follower = null;


    private void Update() {
        // get trigger
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger)) {
            if (follower == null) {
                points.Add(Instantiate(this.Marker, VivePose.GetPose(HandRole.RightHand).pos, Quaternion.identity));
            }
        }

        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad)) {
            if (follower == null) {
                //average points
                Vector3 average = Vector3.zero;
                int count = points.Count;
                foreach (GameObject go in this.points) {
                    average += go.transform.position;
                    Destroy(go);
                }
                this.points.Clear();
                average /= count;

                //make middle point
                if (this.follower != null) {
                    Destroy(this.follower);
                }
                this.follower = Instantiate(this.Follower, average, Quaternion.identity);

                //bind it to controller and hide controller
                foreach (Transform children in this.RightController.transform) {
                    children.gameObject.SetActive(false);
                }

                this.follower.transform.parent = this.RightController.transform;
                this.follower.transform.position = new Vector3(this.follower.transform.position.x+.1f, this.follower.transform.position.y - .275f, this.follower.transform.position.z);

            }
            else {
                //return to initial state
                Destroy(this.follower);
                //bind it to controller and hide controller
                foreach (Transform children in this.RightController.transform) {
                    children.gameObject.SetActive(true);
                }
            }
        }

        if(ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger)) {
            this.follower.transform.Rotate(Vector3.up);
        }
    }
}