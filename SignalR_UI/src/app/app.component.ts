import {Component, OnInit} from '@angular/core';
import {SignalRService} from "./signalR.service";
import {WeatherType} from "./weather.type";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit{
  constructor(private signalRService: SignalRService,) {
  }

  public weather!: WeatherType[];

  ngOnInit(): void {
    this.signalRService.createHubConnection();
    this.signalRService.getWeather().subscribe((res: WeatherType[]) => {
      console.log('*--*--*-', res)
      this.weather = res;
    })

    this.signalRService.weatherAddedOrUpdated.subscribe((messages: WeatherType[]) => {
      console.log('messages', messages)
      if (messages.length) {
        messages.forEach((message) => {
          let index = this.weather.findIndex((s) => s.id === message.id);
          if (index === -1) {
            this.weather.push(message);
          } else {
            this.weather[index] = message;
          }
        });
      }
    });
  }
}
