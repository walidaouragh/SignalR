import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClient} from "@angular/common/http";

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  constructor(private http: HttpClient) {
  }

  private hubConnection!: HubConnection;
  weatherAddedOrUpdated: BehaviorSubject<any> = new BehaviorSubject<any>([]);

  createHubConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:7071' + '/api')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('newMessage', (messages) => {
      this.weatherAddedOrUpdated.next(messages);
    });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch((error) => console.log(error));
  }

  getWeather(): Observable<any>{
    return  this.http.get('http://localhost:5233/WeatherForecast')
  }
}
