using Photon.Pun;
using UnityEngine;

public class ActorEntity : MonoBehaviour
{
    public int index;
    public Transform[] customBodyBones;
    public Transform hip;
    public float sendRatePerSecond = 20f;
    private PhotonView photonView;
    private Quaternion[] boneData;
    private int boneArraySize;
    private Vector3 offSet = Vector3.zero;
    public Vector3 customHeight;
    
    private void Start()
    {
        boneArraySize = customBodyBones?.Length ?? 0;
        boneData = new Quaternion[boneArraySize + 1];
        photonView = GetComponent<PhotonView>();
        InvokeRepeating("SendBoneData", 3f, 1 / sendRatePerSecond);
    }
    
    void SendBoneData()
    {
        PhotonNetwork.RemoveBufferedRPCs();
        if (hip != null)
        {
            boneData[0] = Vector3ToQuaternion(hip.position + NetworkManager.offsetPosition + customHeight);
        }

        for (int i = 0; i < boneArraySize; i++)
        {
            boneData[i + 1] = customBodyBones[i].rotation;
        }

        photonView.RPC("RPCBoneData", RpcTarget.OthersBuffered, boneData);
    }

    private Quaternion Vector3ToQuaternion(Vector3 v)
    {
        return new Quaternion(v.x, v.y, v.z, 0);
    }
}