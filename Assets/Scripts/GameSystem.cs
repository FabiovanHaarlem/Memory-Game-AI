using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSystem : MonoBehaviour
{
    private GameObject[,] m_CardsGrid;

    private List<GameObject> m_Cards;
    private List<GameObject> m_InactiveCards;
    private List<Card> m_AIMemory;
    private List<Card> m_SelectedCards;

    private Sprite[] m_Sprites;

    private Sprite m_CardBackSprite;

    [SerializeField]
    private Text m_Player1ScoreText;
    [SerializeField]
    private Text m_Player2ScoreText;
    [SerializeField]
    private Text m_CurrentPlayingPlayer;

    [SerializeField]
    private GameObject m_StartButton;
    [SerializeField]
    private GameObject m_ResetButton;
    [SerializeField]
    private Text m_SliderText;
    [SerializeField]
    private GameObject m_SetAIMemory;

    private int m_SpriteIndex;
    private int m_AmountOfCards;
    private int m_Player1Score;
    private int m_Player2Score;
    private int m_AIForgetTurn;
    private int m_AIMemorySize;

    private bool m_Player1Turn;
    private bool m_AIActive;


    private void Start()
    {
        m_CardsGrid = new GameObject[4, 5];

        m_Cards = new List<GameObject>();
        m_InactiveCards = new List<GameObject>();
        m_AIMemory = new List<Card>();
        m_SelectedCards = new List<Card>();

        m_Sprites = new Sprite[10];

        m_SpriteIndex = 0;
        m_Player1Score = 0;
        m_Player2Score = 0;
        m_AIForgetTurn = 0;
        m_AIMemorySize = 4;

        m_AmountOfCards = m_Sprites.Length * 2;

        m_Player1Turn = true;
        m_AIActive = false;

        m_CardBackSprite = Resources.Load<Sprite>("Sprites\\CardBack");
        m_Sprites = Resources.LoadAll<Sprite>("Sprites\\Cards");

        m_ResetButton.SetActive(false);
        m_SetAIMemory.SetActive(false);
    }

    public void StartGame()
    {
        m_StartButton.SetActive(false);
        m_ResetButton.SetActive(true);
        m_CurrentPlayingPlayer.text = "Turn: Player 1";
        MakeCards();
    }

    public void StopGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    public void SetAIMemorySize(Slider slider)
    {
        m_AIMemorySize = (int)slider.value;
    }

    public void SwitchGameMode(int currentGameMode)
    {
        if (currentGameMode == 0)
        {
            m_AIActive = false;
            m_SetAIMemory.SetActive(false);
        }
        else if (currentGameMode == 1)
        {
            m_AIActive = true;
            m_SetAIMemory.SetActive(true);
        }
    }

    private void MakeCards()
    {
        for (int i = 0; i < m_AmountOfCards; i++)
        {
            m_SpriteIndex += 1;

            m_Cards.Add(new GameObject());
            m_Cards[i].AddComponent<Card>();
            m_Cards[i].AddComponent<SpriteRenderer>();
            m_Cards[i].AddComponent<BoxCollider>();
            m_Cards[i].gameObject.tag = "Card";

            if (m_SpriteIndex == 10)
            {
                m_SpriteIndex = 0;
            }

            m_Cards[i].GetComponent<Card>().SetCardSprite(m_Sprites[m_SpriteIndex], m_CardBackSprite);
            m_Cards[i].name = "Pokemon" + i;
            m_InactiveCards.Add(m_Cards[i]);
        }
        PlaceCards();
    }

    private void PlaceCards()
    {
        Vector3[,] cardsPositions2DArray = new Vector3[m_AmountOfCards, 4];
        List<Vector3> cardsPositionsList = new List<Vector3>();

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                cardsPositions2DArray[j, i] = new Vector3(-6 + (2 * (j + 1)), -5 + (2 * (i + 1)), 0);
                cardsPositionsList.Add(cardsPositions2DArray[j, i]);
            }
        }

        for (int i = 0; i < m_AmountOfCards; i++)
        {
            int randomIndexCards = Random.Range(0, m_Cards.Count);
            int randomIndexPositions = Random.Range(0, cardsPositionsList.Count);

            m_Cards[randomIndexCards].GetComponent<Card>().SetPosition(cardsPositionsList[randomIndexPositions]);
            m_Cards.RemoveAt(randomIndexCards);
            cardsPositionsList.RemoveAt(randomIndexPositions);
        }
    }

    private void Update()
    {
        m_SliderText.text = "AI Difficulty: " + m_AIMemorySize;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectCard();

            for (int i = 0; i < m_SelectedCards.Count; i++)
            {
                m_SelectedCards[i].ShowCard();
                m_SelectedCards[i].gameObject.layer = 2;
            }

            if (m_SelectedCards.Count == 2)
            {
                StartCoroutine(CheckCards());
            }
        }
        m_Player1ScoreText.text = "Player 1 Score: " + m_Player1Score;
        m_Player2ScoreText.text = "Player 2 Score: " + m_Player2Score;
    }

    private void SelectCard()
    {
        RaycastHit hit;

        Ray ray = (Camera.main.ScreenPointToRay(Input.mousePosition));

        Debug.DrawRay(Camera.main.transform.position, ray.direction * 50, Color.red);

        if (m_SelectedCards.Count != 2)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.CompareTag("Card"))
                {
                    m_SelectedCards.Add(hit.collider.gameObject.GetComponent<Card>());
                }
            }
        }
    }

    private void AITurn()
    {
        if (m_SelectedCards.Count == 0)
        {
            if (m_AIMemory.Count > m_AIMemorySize)
            {
                int memoryOverload = m_AIMemory.Count - (m_AIMemorySize - 1);

                for (int i = 1; i < memoryOverload; i++)
                {
                    m_AIMemory.RemoveAt(m_AIMemory.Count - i);
                }
            }
        }

        if (m_SelectedCards.Count == 0)
        {
            if (m_AIMemory.Count >= 1)
            {
                for (int i = 0; i < m_AIMemory.Count; i++)
                {
                    for (int j = 0; j < m_AIMemory.Count; j++)
                    {
                        if (i != j)
                        {
                            if (m_AIMemory[i].GetComponent<Card>().GetSprite.name == m_AIMemory[j].GetComponent<Card>().GetSprite.name)
                            {
                                if (m_AIMemory[i].GetComponent<Card>().GetInstanceID() != m_AIMemory[j].GetComponent<Card>().GetInstanceID())
                                {
                                    m_SelectedCards.Add(m_AIMemory[i].GetComponent<Card>());
                                    m_SelectedCards.Add(m_AIMemory[j].GetComponent<Card>());
                                    break;
                                }
                            }
                        }
                    }
                    if (m_SelectedCards.Count == 2)
                    {
                        break;
                    }
                }
            }
        }


        if (m_SelectedCards.Count == 0)
        {
            int randomIndexChose = 0;

            if (m_AIMemory.Count >= m_AIMemorySize)
            {
                randomIndexChose = Random.Range(0, m_InactiveCards.Count);

                m_SelectedCards.Add(m_InactiveCards[randomIndexChose].GetComponent<Card>());

                for (int i = 0; i < m_AIMemory.Count; i++)
                {
                    if (m_SelectedCards[0].GetInstanceID() != m_AIMemory[i].GetInstanceID())
                    {
                        if (m_SelectedCards[0].GetSprite.name == m_AIMemory[i].GetSprite.name)
                        {
                            Debug.Log("Got One");
                            m_SelectedCards.Add(m_AIMemory[i]);
                            break;
                        }
                    }
                }

                if (m_SelectedCards.Count == 1)
                {
                    m_SelectedCards.Clear();
                }
            }
        }

        if (m_SelectedCards.Count == 0)
        {
            if (m_InactiveCards.Count >= 1)
            {
                int randomIndexChose1 = -1;
                int randomIndexChose2 = -1;
                bool randomfound = false;
                while(randomfound == false)
                {
                    randomIndexChose1 = Random.Range(0, m_InactiveCards.Count);
                    randomIndexChose2 = Random.Range(0, m_InactiveCards.Count);

                    if(randomIndexChose1 != randomIndexChose2 && m_InactiveCards[randomIndexChose1] != null && m_InactiveCards[randomIndexChose2] != null)
                    {
                        if(m_InactiveCards[randomIndexChose1].gameObject.activeInHierarchy && m_InactiveCards[randomIndexChose2].gameObject.activeInHierarchy)
                        {
                            randomfound = true;
                        }
                    }
                }
                

                m_SelectedCards.Add(m_InactiveCards[randomIndexChose1].GetComponent<Card>());
                m_SelectedCards.Add(m_InactiveCards[randomIndexChose2].GetComponent<Card>());
            }
        }

        if (m_SelectedCards.Count == 2)
        {
            for (int i = 0; i < m_SelectedCards.Count; i++)
            {
                m_SelectedCards[i].ShowCard();
            }
        }
        else
        {
            m_SelectedCards.Clear();
            AITurn();
        }

        if (m_SelectedCards.Count == 2)
        {
            StartCoroutine(CheckCards());
        }
    }

    private void SwitchTurn()
    {
        for (int i = 0; i < m_SelectedCards.Count; i++)
        {
            m_SelectedCards[i].gameObject.layer = 0;
            m_SelectedCards[i].HideCard();

        }

        m_SelectedCards.Clear();
        
        m_Player1Turn = !m_Player1Turn;

        CheckWin();
        SwitchTurnText();

        if (m_AIActive)
        {
            if (!m_Player1Turn)
            {
                AITurn();
            }
        }
    } 

    private void PickAgain()
    {
        m_SelectedCards.Clear();
        CheckWin();
        if (m_AIActive)
        {
            if (!m_Player1Turn)
            {
                int cardsActive = 0;

                for (int i = 0; i < m_InactiveCards.Count; i++)
                {
                    if (m_InactiveCards[i].gameObject.activeInHierarchy == true)
                    {
                        cardsActive++;
                    }
                }

                if(cardsActive >= 1)
                {
                    AITurn();
                }          
            }
        }
    }

    private void CheckWin()
    {
        for(int i  = 0; i< m_InactiveCards.Count; i++)
        {
            if(m_InactiveCards[i].gameObject.activeInHierarchy == true)
            {
                return;
            }
        }

        if (m_Player1Score > m_Player2Score)
        {
            m_CurrentPlayingPlayer.text = "Player 1 Won!!!";
        }
        else if (m_Player2Score > m_Player1Score)
        {
            m_CurrentPlayingPlayer.text = "Player 2 Won!!!";
        }
        else if (m_Player2Score == m_Player1Score)
        {
            m_CurrentPlayingPlayer.text = "It's A Draw!!!";
        }
        Time.timeScale = 0;
    }

    private void SwitchTurnText()
    {
        if (m_Player1Turn)
        {
            if (m_InactiveCards.Count != 0)
            {
                m_CurrentPlayingPlayer.text = "Turn: Player 1";
            }
        }
        else if (!m_Player1Turn)
        {
            if (!m_AIActive)
            {
                if (m_InactiveCards.Count != 0)
                {
                    m_CurrentPlayingPlayer.text = "Turn: Player 2";
                }
            }
            else if (m_AIActive)
            {
                if (m_InactiveCards.Count != 0)
                {
                    m_CurrentPlayingPlayer.text = "Turn: AI";
                }
            }
        }
    }

    private void RemoveSelectedCardsFromAIMemory()
    {
        for (int i = 0; i < m_SelectedCards.Count; i++)
        {
            for (int j = 0; j < m_AIMemory.Count; j++)
            {
                if (m_SelectedCards[i].GetInstanceID() == m_AIMemory[j].GetInstanceID())
                {
                    m_AIMemory.RemoveAt(j);
                    break;
                }
            }
        }
    }

    private IEnumerator CheckCards()
    {
        yield return new WaitForSeconds(1.5f);

        RemoveSelectedCardsFromAIMemory();
        if (m_InactiveCards.Count >= 2)
        {
            if (m_SelectedCards.Count == 2)
            {
                if (m_SelectedCards[0].GetSprite.name == m_SelectedCards[1].GetSprite.name)
                {
                    for (int i = 0; i < m_SelectedCards.Count; i++)
                    {
                        m_SelectedCards[i].gameObject.SetActive(false);
                    }

                    if (m_Player1Turn)
                    {
                        m_Player1Score++;
                        PickAgain();
                    }
                    else
                    {
                        m_Player2Score++;
                        PickAgain();
                    }
                }
                else
                {
                    for (int i = 0; i < m_SelectedCards.Count; i++)
                    {
                        m_AIMemory.Add(m_SelectedCards[i]);
                    }

                    SwitchTurn();
                }
            }
        }
    }
}
