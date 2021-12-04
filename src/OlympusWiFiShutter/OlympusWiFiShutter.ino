#include <ESP8266HTTPClient.h>
#include <ESP8266WiFi.h>
#include "Config.h"

int buttonState = 0;
int lastButtonState = 0;
unsigned long previousMillis = 0;
unsigned long interval = 30000;

void initWiFi() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(CAMERA_SSID, CAMERA_PASS);

  IPAddress local_IP(192, 168, 1, 10);
  IPAddress gateway(192, 168, 1, 1);
  
  IPAddress subnet(255, 255, 0, 0);
  IPAddress primaryDNS(8, 8, 8, 8);   // optional
  IPAddress secondaryDNS(8, 8, 4, 4); // optional
  if (!WiFi.config(local_IP, gateway, subnet, primaryDNS, secondaryDNS)) {
    Serial.println("STA Failed to configure");
  }
  
  Serial.print("Connecting to WiFi ..");
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print('.');
    delay(1000);
  }
  Serial.println("");
  Serial.println("Local IP:");
  Serial.println(WiFi.localIP());
}

void setup()
{
  Serial.begin(115200);
  pinMode(SHUTTER_PIN, INPUT_PULLUP);

  initWiFi();
  Serial.println("RRSI: ");
  Serial.println(WiFi.RSSI());
}


void loop()
{
  unsigned long currentMillis = millis();
  // if WiFi is down, try reconnecting every CHECK_WIFI_TIME seconds
  if ((WiFi.status() != WL_CONNECTED) && (currentMillis - previousMillis >=interval)) {
    WiFi.disconnect();
    initWiFi();
  }
  
  buttonState = digitalRead(SHUTTER_PIN);

  if (buttonState != lastButtonState) {
    if (digitalRead(SHUTTER_PIN) == LOW)
    {
      int httpCode;

      Serial.println("Taking the picture...");
      {
        WiFiClient client;
        HTTPClient http;
        http.begin(client,"http://192.168.1.20:5001/OlympusCamera/TakePictureAllInOne");
        httpCode = http.POST("");
        http.end();
      }
      
      // Delay a little bit to avoid bouncing
      delay(50);
    }
  }
  lastButtonState = buttonState;
}
