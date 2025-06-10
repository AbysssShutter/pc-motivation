using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ScrollZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject sliderMask;
    [SerializeField] private Slider slider;
    [SerializeField] private Camera worldCam;
    [SerializeField] private RectTransform handleSize;
    private bool cursosIn = false;
    private float currentScroll = 0f;
    private float lastPos = 0f;
    void Update()
    {
        if (cursosIn)
        {
            //Debug.Log(slider.transform.localScale * );
            currentScroll = Input.GetAxis("Mouse ScrollWheel");
            if (currentScroll != 0)
            {
                if (slider.transform.localScale.x <= 1 && currentScroll < 0) { }
                else if (slider.transform.localScale.x >= 5 && currentScroll > 0) { }
                else if (slider.transform.localPosition.x != 0 && currentScroll < 0)
                {
                    if (slider.transform.localPosition.x <= 10 && slider.transform.localPosition.x >= -10)
                    {
                        slider.transform.localPosition = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        slider.transform.localScale = new Vector3(
                            slider.transform.localScale.x + currentScroll,
                            slider.transform.localScale.y,
                            slider.transform.localScale.z);
                        slider.transform.localPosition = new Vector3(slider.transform.localPosition.x / 4, 0, 0);
                    }
                }
                else
                {
                    slider.transform.localScale = new Vector3(
                            slider.transform.localScale.x + currentScroll,
                            slider.transform.localScale.y,
                            slider.transform.localScale.z);
                }
                handleSize.localScale = new Vector3(
                        1 / slider.transform.localScale.x,
                        handleSize.localScale.y,
                        handleSize.localScale.z);
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        cursosIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursosIn = false;
    }

    public void DragSlider()
    {
        if (slider.transform.localScale.x > 1)
        {
            Vector2 currMousePos = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderMask.transform.GetComponent<RectTransform>(), Input.mousePosition, worldCam, out currMousePos);
            if (lastPos != 0)
            {
                if (currMousePos[0] > lastPos)
                {
                    slider.transform.localPosition = new Vector3(slider.transform.localPosition.x - 0.1f * Math.Abs(currMousePos[0]), 0, 0);
                }
                else if (currMousePos[0] < lastPos)
                {
                    slider.transform.localPosition = new Vector3(slider.transform.localPosition.x + 0.1f * Math.Abs(currMousePos[0]), 0, 0);
                }
            }
            lastPos = currMousePos[0];   
        }
    }
}
