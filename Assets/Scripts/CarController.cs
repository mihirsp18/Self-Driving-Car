using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    public TextMeshProUGUI Status;
    public TextMeshProUGUI AvgSpeedLable;
    public Slider ProgressBar;
    private Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float fitnessGoal = 2500f;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 1f;
    public float sensorMultiplier = 0.1f;
    public float speedCutOff = 5f;
    public float MaxSpeed = 45f;
    public float curv = 0.05f;


    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    [Header("Button Ref")]
    public Button TerminateButton;
    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();
  
    }
    void Start()
    {
        TerminateButton.onClick.AddListener(Death);
        TerminateButton.gameObject.SetActive(false);
    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }


    public void Reset()
    {   
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }

    private void FixedUpdate()
    {
        ProgressBar.value = overallFitness / fitnessGoal;
        AvgSpeedLable.SetText((avgSpeed*1.5).ToString("0"));

        InputSensors();
        lastPosition = transform.position;


        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);


        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();

    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
        Status.SetText("Fitness Bar:");
        TerminateButton.gameObject.SetActive(false);
    }

    private void CalculateFitness()
    {

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 5 && overallFitness < speedCutOff*5)
        {
            Death();
        }

        if (overallFitness >= fitnessGoal)
        {
            //Saves network to a JSON
            Status.SetText("Successfully Trained!");
            TerminateButton.gameObject.SetActive(true);
            //Death();
        }
        // if (overallFitness >= (fitnessGoal + 500))
        // {
        //     //Saves network to a JSON
        //     Status.SetText("Fitness Bar:");
        //     
        // }

    }

    private void InputSensors()
    {

        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            var h = hit.distance;
            if (h > 30)
            {
                aSensor = 1;
            }
            else
            {
                aSensor = hit.distance / 30;
            }
            Debug.DrawLine(r.origin, hit.point, Color.green);
            print("A: " + aSensor);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            var h = hit.distance;
            if (h > 30)
            {
                bSensor = 1;
            }
            else
            {
                bSensor = hit.distance / 30;
            }
            Debug.DrawLine(r.origin, hit.point, Color.green);
            print("B: " + bSensor);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            var h = hit.distance;
            if (h > 30)
            {
                cSensor = 1;
            }
            else
            {
                cSensor = hit.distance / 30;
            }
            Debug.DrawLine(r.origin, hit.point, Color.green);
            print("C: " + cSensor);
        }

    }

    private Vector3 inp;
    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * MaxSpeed), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h * 70) * curv, 0);
    }

}
