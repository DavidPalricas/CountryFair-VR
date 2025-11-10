using UnityEngine;
using Oculus.Interaction.Input;

public class BowHandTracking : MonoBehaviour
{
    [Header("References")]
    public OVRHand leftHand;
    public OVRHand rightHand;
    public Transform stringStart;
    public Transform stringEnd;
    public LineRenderer bowString;
    public Transform arrowSpawn;
    public GameObject arrowPrefab;

    [Header("Settings")]
    public float maxForce = 25f;
    public float maxDrawDistance = 0.5f;

    private GameObject currentArrow;
    private bool isDrawing = false;
    private Vector3 initialStringPos;

    void Start()
    {
        initialStringPos = stringEnd.localPosition;
        bowString.positionCount = 2;
    }

    void Update()
    {
        // Posição do arco segue a mão esquerda
        transform.position = leftHand.transform.position;
        transform.rotation = leftHand.transform.rotation;

        // Verifica se a mão direita está a fazer punho (fechada)
        bool rightHandClosed = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.8f &&
                               rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) > 0.8f &&
                               rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) > 0.8f;

        if (rightHandClosed && !isDrawing)
        {
            StartDrawing();
        }

        if (isDrawing)
        {
            UpdateDraw();

            // Larga a corda (quando abre a mão)
            if (!rightHandClosed)
            {
                ReleaseArrow();
            }
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        currentArrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation);
    }

    void UpdateDraw()
    {
        Vector3 pullDir = (rightHand.transform.position - stringStart.position);
        float drawDist = Mathf.Min(pullDir.magnitude, maxDrawDistance);

        // Atualiza posição da corda
        stringEnd.position = stringStart.position + pullDir.normalized * drawDist;

        bowString.SetPosition(0, stringStart.position);
        bowString.SetPosition(1, stringEnd.position);

        // Atualiza posição da flecha (segue o ponto de corda)
        if (currentArrow)
        {
            currentArrow.transform.position = arrowSpawn.position;
            currentArrow.transform.rotation = transform.rotation;
        }
    }

    void ReleaseArrow()
    {
        isDrawing = false;
        bowString.SetPosition(0, stringStart.position);
        bowString.SetPosition(1, stringStart.position);

        if (currentArrow)
        {
            Vector3 direction = transform.forward;
            float drawForce = Vector3.Distance(stringStart.position, stringEnd.position) / maxDrawDistance;
            currentArrow.GetComponent<Arrow>().Launch(direction, drawForce * maxForce);
            currentArrow = null;
        }

        // Reset à corda
        stringEnd.localPosition = initialStringPos;
    }
}
