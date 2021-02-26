#include <Adafruit_MLX90614.h>  // for infrared thermometer
Adafruit_MLX90614 mlx = Adafruit_MLX90614();  //for infrared thermometer

double temp;  // to save temperature value
const int trigPin = 3;  //ultrasonic
const int echoPin = 2;  //ultrasonic
const int ledPin = 9;
long duration;
int distance;
int step1_judge = 0;
int curStatus = 0;
String inputString = "";         // a String to hold incoming data
bool stringComplete = false;  // whether the string is complete

void setup() {

  Serial.begin(9600);
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
    pinMode(ledPin, OUTPUT);
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  mlx.begin();  //start infrared thermometer
  delay(100);
}

void loop() {
  if (stringComplete) {
    //------------reading distance
    // Sets the trigPin on HIGH state for 10 micro seconds
    digitalWrite(trigPin, HIGH);
    digitalWrite(ledPin, HIGH);
    delayMicroseconds(10);
    digitalWrite(trigPin, LOW);
    // Reads the echoPin, returns the sound wave travel time in microseconds
    duration = pulseIn(echoPin, HIGH, 23529); //23529us for timeout 4.0m

    // Calculating the distance
    distance = duration * 0.034 / 2;

    if ((distance < 10) && (distance > 0)) step1_judge++;
    else step1_judge = 0;

    if (step1_judge > 2) {
      step1_judge = 0;

      temp = 8.5 + round(mlx.readObjectTempC() * 10) * .1; //---------------------reading temperature & show on LCD
      //temp = 37.4;  //for testing, comment this line for real reading

      Serial.println(temp);
      stringComplete = false;
      inputString = "";
      delay(2000);
      digitalWrite(ledPin, LOW);
    }
    delay(300);
  }
}

/*
  SerialEvent occurs whenever a new data comes in the hardware serial RX. This
  routine is run between each time loop() runs, so using delay inside loop can
  delay response. Multiple bytes of data may be available.
*/
void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inChar == '\n') {
      if(inputString == "2\n"){
      stringComplete = false;
      }
      else
      stringComplete = true;
    }
  }
}
