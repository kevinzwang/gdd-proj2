using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    #region Variables
    Rigidbody2D playerRigidbody;
    HashSet<Vector3Int> firePositions;

    [SerializeField]
    [Tooltip("The tilemap that contains the walls")]
    private Tilemap walls;

    [SerializeField]
    [Tooltip("The fire prefab")]
    private GameObject firePrefab;

    [SerializeField]
    [Tooltip("The starting energy")]
    private int startEnergy = 8;

    [SerializeField]
    [Tooltip("The amount of energy a health item gives")]
    private int energyPerItem = 5;

    [SerializeField]
    [Tooltip("Energy level text UI")]
    private TMP_Text energyText;



    int energy;

    bool moving;
    #endregion


    #region Methods
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        firePositions = new HashSet<Vector3Int>();
        energy = startEnergy + 1;
    }

    private void Start()
    {
        AddFire();
        energyText.text = energy.ToString();
        moving = false;
    }

    private void Update()
    {
        Vector3 movementVector = new Vector3(0, 0, 0);
        if (!moving) {
            if (Input.GetKeyDown(KeyCode.W)) {
                movementVector.y = 1;
            } else if (Input.GetKeyDown(KeyCode.S)) {
                movementVector.y = -1;
            } else if (Input.GetKeyDown(KeyCode.A)) {
                movementVector.x = -1;
            } else if (Input.GetKeyDown(KeyCode.D)) {
                movementVector.x = 1;
            }
        }

        Vector3 newPostion = transform.position + movementVector;
        Vector3Int cellPosition = walls.WorldToCell(newPostion);
        if (movementVector != Vector3.zero && !walls.HasTile(cellPosition)) {
            StartCoroutine(Move(movementVector));
        }
    }

    private void AddFire()
    {
        Vector3Int cellPosition = walls.WorldToCell(transform.position);
        if (!firePositions.Contains(cellPosition)) {
            firePositions.Add(cellPosition);
            Instantiate(firePrefab, transform.position, Quaternion.identity);
            energy--;
        }
    }

    IEnumerator Move(Vector3 movementVector)
    {
        moving = true;
        Vector3 oldPosition = transform.position;
        Vector3 newPosition = transform.position + movementVector;
        
        float elapsedTime = 0;
        float animationTime = 0.1f;
        while (elapsedTime < animationTime) {
            transform.position = Vector3.Lerp(oldPosition, newPosition, elapsedTime / animationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = newPosition;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.CompareTag("Health")) {
                energy += energyPerItem;
                Destroy(hit.collider.gameObject);
            } else if (hit.collider.CompareTag("Goal")) {
                Destroy(this.gameObject);
                SceneManager.LoadScene("WinScene");
            }
        }

        AddFire();

        if (energy == 0) {
            Destroy(this.gameObject);
            SceneManager.LoadScene("LoseScene");
        }

        energyText.text = energy.ToString();

        moving = false;
    }
    #endregion
}
