using Shared.Move;
using UdpToolkit.Core;
using UdpToolkit.Framework.Client.Core;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    [SerializeField]
    private float forwardForce = 100;

    [SerializeField]
    private float sidewaysForce = 100;

    [SerializeField]
    private float jumpForce = 15;

    [SerializeField]
    private int jumpLimit = 2;

    private int totalJumps = 0;

    public byte PlayerId { get; set; }

    private new Rigidbody rigidbody;

    private Vector3 prevPosition;

    public IClientHost ClientHost { get; set; }

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        prevPosition = transform.position;
    }

    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.Space))
         {
             if (totalJumps < jumpLimit)
             {
                 rigidbody.velocity = new Vector3(0f, jumpForce, 0f);
                 totalJumps += 1;
             }
         }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag($"Ground"))
        {
            totalJumps = 0;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddForce(sidewaysForce * Time.deltaTime, 0f, 0f, ForceMode.VelocityChange);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddForce(-sidewaysForce * Time.deltaTime, 0f, 0f, ForceMode.VelocityChange);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(0f, 0f, forwardForce * Time.deltaTime, ForceMode.VelocityChange);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce(0f, 0f, -forwardForce * Time.deltaTime, ForceMode.VelocityChange);
        }

        // network
        SendPosition();
        prevPosition = transform.position;
    }

    private void SendPosition()
    {
        // todo delta for jitter
        if (prevPosition != transform.position)
        {
            var @event = new MoveEvent
            {
                PlayerId = PlayerId,
                Rotation = transform.rotation,
                Position = transform.position,
            };

            ClientHost.Publish(@event, hubId: 0,  rpcId: 2, UdpMode.Udp);
        }
    }
}
