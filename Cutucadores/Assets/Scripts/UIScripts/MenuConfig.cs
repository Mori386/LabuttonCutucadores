using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuConfig : MonoBehaviour
{
    public Slider slider;
    public Image[] images;

    // Variável para armazenar o volume atual
    private float currentVolume = 1.0f; // 1.0f representa o volume máximo
    public void Exit()
    {
        Application.Quit();
    }
    private void Start()
    {
        // Carregar o valor do volume salvo (se existir)
        if (PlayerPrefs.HasKey("Volume"))
        {
            currentVolume = PlayerPrefs.GetFloat("Volume");
        }

        // Atualizar o slider e o volume
        slider.value = currentVolume;
        AudioListener.volume = currentVolume;
    }

    private void Update()
    {
        //com base no valor do volume, ativa e desativa as imagens
        for (int i = 0; i < images.Length; i++)
        {
            float ImagesActivations = (i + 1) * 0.25f;

            if (currentVolume >= ImagesActivations)
            {
                images[i].enabled = true; // Ativa a imagem
            }
            else
            {
                images[i].enabled = false; // Desativa a imagem
            }
        }
    }

    public void more(float value) //vinculado no botão de mais
    {

        slider.value += value;
        currentVolume = Mathf.Clamp01(currentVolume + value); // transformar o ponto fluante para ser compativel com 0,25
        AudioListener.volume = currentVolume;

        // Salvar o novo valor do volume
        PlayerPrefs.SetFloat("Volume", currentVolume);
        PlayerPrefs.Save();

        Debug.Log("O volume atual é:" + PlayerPrefs.GetFloat("Volume"));
    }

    public void any(float value) //vinculado no botão de menos
    {

        slider.value -= value;
        currentVolume = Mathf.Clamp01(currentVolume - value); // transformar o ponto fluante para ser compativel com 0,25
        AudioListener.volume = currentVolume;

       
        PlayerPrefs.SetFloat("Volume", currentVolume);
        PlayerPrefs.Save();

        Debug.Log("O volume atual é:" + PlayerPrefs.GetFloat("Volume"));
    }

    public void Fullscreen(bool kitten)
    {
        Screen.fullScreen = true;
    }

    public void Janela(bool kitten)
    {
        Screen.SetResolution(1280, 720, false);
        
    }
}
