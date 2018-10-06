using UnityEngine;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;
using System.Linq;

public class ControllerManager : MonoBehaviour {

    public GameObject Marker;
    public List<GameObject> Followers;
    public GameObject LeftController;
    public GameObject RightController;
    public List<Material> Materials;

    private int materialIndex = 0;
    private int followerIndex = 0;
    private bool leftFollower = false;
    private bool rightFollower = false;
    private List<GameObject> points = new List<GameObject>();
    private GameObject follower = null;
    private Vector3 followerCenter = Vector3.zero;
    private float followerScale = 0f;

    private Vector3 calculateCenter(List<GameObject> points) {
        //average points
        Vector3 center = Vector3.zero;
        int count = points.Count;
        foreach (GameObject go in points) {
            center += go.transform.position;
        }
        center /= count;
        return center;
    }

    private float scaleObjectBasedOnPoints(GameObject gameobject, List<GameObject> points) {
        Vector3 center3 = calculateCenter(points);
        Vector2 center2 = new Vector2(center3.x, center3.z);

        //find distance from center
        float scale = 0;
        foreach (GameObject go in points) {
            Vector2 v = new Vector2(go.transform.position.x, go.transform.position.z);
            scale += Mathf.Abs(Vector2.Distance(center2, v));
        }
        scale /= 4;
        //adjust for cube scale
        scale *= 2;
        scale /= Mathf.Sqrt(2);
        //scale box
        gameobject.transform.localScale = Vector3.one * scale;
        return scale;
    }

    private void updateFollower() {
        Transform parent;
        if (this.follower != null) {
            parent = this.follower.transform.parent;
            Destroy(this.follower);
        }
        else {
            Debug.Log("Bad State");
            return;
        }
        //set dimensions
        this.follower = Instantiate(this.Followers[this.followerIndex], this.followerCenter, Quaternion.identity);
        this.follower.transform.localScale = Vector3.one * this.followerScale;
        this.follower.transform.position = new Vector3(this.follower.transform.position.x, this.follower.transform.position.y - this.followerScale / 2, this.follower.transform.position.z);

        this.updateFollowerMaterial();

        //reattach to parent
        this.follower.transform.parent = parent;
    }

    private void cycleFollower() {
        this.followerIndex++;
        this.followerIndex %= this.Followers.Count;
        this.updateFollower();
    }

    private void makeFollower(GameObject toFollow, ref bool whichFollower) {
        if (this.follower == null) {
            if (this.points.Count == 4) {
                //average points
                this.followerCenter = this.calculateCenter(this.points);

                //make follower at middle point
                if (this.follower != null) {
                    Destroy(this.follower);
                }
                this.follower = Instantiate(this.Followers[this.followerIndex], this.followerCenter, Quaternion.identity);

                //rotate
                /*float rot = 0;
                foreach(GameObject go in this.points) {
                    Vector2 v = new Vector2(go.transform.position.x, go.transform.position.z) - center2;
                    float ang = Vector2.SignedAngle(Vector2.one, v);
                    ang = ang > 0 ? ang : 360f + ang;
                    Debug.Log(ang % 90);
                    ang %= 90;
                    ang = ang > 45 ? ang - 90 : ang;
                    rot += ang % 90;
                }
                rot /= 4;
                this.follower.transform.Rotate(0f, rot, 0f, Space.World);*/

                this.followerScale = this.scaleObjectBasedOnPoints(this.follower, this.points);

                //move box to line up with top of object
                this.follower.transform.position = new Vector3(this.follower.transform.position.x, this.follower.transform.position.y - this.followerScale / 2, this.follower.transform.position.z);


                //bind it to controller and hide controller
                foreach (Transform children in toFollow.transform) {
                    children.gameObject.SetActive(false);
                }
                whichFollower = true;
                this.updateFollowerMaterial();


                this.follower.transform.parent = toFollow.transform;
                //this.follower.transform.position = new Vector3(this.follower.transform.position.x+.1f, this.follower.transform.position.y - .275f, this.follower.transform.position.z);

                foreach (GameObject go in this.points) {
                    Destroy(go);
                }
                this.points.Clear();
            }
            else {
                foreach (GameObject go in this.points) {
                    Destroy(go);
                }
                this.points.Clear();
            }
        }
        else {
            //return to initial state
            Destroy(this.follower);
            //bind it to controller and hide controller
            foreach (Transform children in toFollow.transform) {
                children.gameObject.SetActive(true);
            }
            whichFollower = false;
        }
    }

    private void spawnMarker(Vector3 pos) {
        if (this.follower == null) {
            if (this.points.Count < 4) {
                this.points.Add(Instantiate(this.Marker, pos, Quaternion.identity));
            }
            else if (this.points.Count == 4) {
                Destroy(this.points[0]);
                this.points.RemoveAt(0);
                this.points.Add(Instantiate(this.Marker, pos, Quaternion.identity));
            }
            else {
                foreach (GameObject go in this.points) {
                    Destroy(go);
                }
                this.points.Clear();
            }
        }
    }

    private void updateFollowerMaterial() {
        if (this.follower != null) {
            this.follower.GetComponent<Renderer>().material = this.Materials[this.materialIndex];
        }
        else {
            Debug.Log("Bad State");
        }
    }

    private void cycleFollowerMaterial() {
        this.materialIndex++;
        this.materialIndex %= this.Materials.Count;
        this.updateFollowerMaterial();
    }

    private void Update() {
        // get trigger
        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger) && !this.leftFollower && !this.rightFollower) {
            this.spawnMarker(VivePose.GetPose(HandRole.LeftHand).pos);
        }
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger) && !this.leftFollower && !this.rightFollower) {
            this.spawnMarker(VivePose.GetPose(HandRole.RightHand).pos);
        }

        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger) && (this.leftFollower || this.rightFollower)) {
            this.cycleFollowerMaterial();
        }
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger) && (this.leftFollower || this.rightFollower)) {
            this.cycleFollowerMaterial();
        }

        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Pad) && !this.rightFollower) {
            this.makeFollower(this.LeftController, ref this.leftFollower);
        }
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad) && !this.leftFollower) {
            this.makeFollower(this.RightController, ref this.rightFollower);
        }

        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Pad) && this.rightFollower) {
            this.cycleFollower();
        }
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad) && this.leftFollower) {
            this.cycleFollower();
        }

        /*if ((this.leftFollower || this.rightFollower) && (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger) || ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger))) {
            this.follower.transform.Rotate(Vector3.up);
        }*/
    }
}