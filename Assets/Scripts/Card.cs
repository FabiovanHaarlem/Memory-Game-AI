using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Collider))]

public class Card : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;

    private Sprite m_CardSprite;
    public Sprite GetSprite { get { return m_CardSprite; } }
    private Sprite m_CardBackSprite;


    public void ShowCard()
    {
        m_SpriteRenderer.sprite = m_CardSprite;
    }

    public void HideCard()
    {
        m_SpriteRenderer.sprite = m_CardBackSprite;
    }

    public void SetCardSprite(Sprite cardSprite, Sprite cardBackSprite)
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        //m_SpriteRenderer.sprite = cardSprite;
        m_SpriteRenderer.sprite = cardBackSprite;
        m_CardSprite = cardSprite;
        m_CardBackSprite = cardBackSprite;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
