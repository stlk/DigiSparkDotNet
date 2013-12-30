#define USB_CFG_DEVICE_NAME     'D','i','g','i','S','h','a','r','p'
#define USB_CFG_DEVICE_NAME_LEN 9
#define LED 1 // any PWM led will do
#include <DigiUSB.h>

boolean breathing = true;

unsigned long status_breathe_time = millis();
int breathe_delay = 10;
boolean breathe_up = true;
int breathe_i = 15;

void setup() {
  DigiUSB.begin();
  pinMode(1, OUTPUT);
}

void loop() {
  DigiUSB.refresh();
  if (DigiUSB.available() > 0) {
    breathing = DigiUSB.read() == 1;

    if(!breathing) {
      analogWrite(LED, 0);
    }

    if(breathing) {
      DigiUSB.write("1");
    }
    else{
      DigiUSB.write("0");
    }
  }

  if(breathing) {
    nonBlockingBreathe(); 
  }
}


void nonBlockingBreathe(){
  if( (status_breathe_time + breathe_delay) < millis() ){
    analogWrite(LED, breathe_i);
    status_breathe_time = millis();
    if (breathe_up == true){
      if (breathe_i > 150) {
        breathe_delay = 4;
      }
      if ((breathe_i > 125) && (breathe_i < 151)) {
        breathe_delay = 5;
      }
      if (( breathe_i > 100) && (breathe_i < 126)) {
        breathe_delay = 7;
      }
      if (( breathe_i > 75) && (breathe_i < 101)) {
        breathe_delay = 10;
      }
      if (( breathe_i > 50) && (breathe_i < 76)) {
        breathe_delay = 14;
      }
      if (( breathe_i > 25) && (breathe_i < 51)) {
        breathe_delay = 18;
      }
      if (( breathe_i > 1) && (breathe_i < 26)) {
        breathe_delay = 19;
      }
      breathe_i += 1;
      if( breathe_i >= 255 ){
        breathe_up = false;
      }
    }
    else{
      if (breathe_i > 150) {
        breathe_delay = 4;
      }
      if ((breathe_i > 125) && (breathe_i < 151)) {
        breathe_delay = 5;
      }
      if (( breathe_i > 100) && (breathe_i < 126)) {
        breathe_delay = 7;
      }
      if (( breathe_i > 75) && (breathe_i < 101)) {
        breathe_delay = 10;
      }
      if (( breathe_i > 50) && (breathe_i < 76)) {
        breathe_delay = 14;
      }
      if (( breathe_i > 25) && (breathe_i < 51)) {
        breathe_delay = 18;
      }
      if (( breathe_i > 1) && (breathe_i < 26)) {
        breathe_delay = 19;
      }
      breathe_i -= 1;
      if( breathe_i <= 15 ){
        breathe_up = true;
        breathe_delay = 400;
      }
    }
  }
}