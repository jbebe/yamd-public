import {Component, Input, OnInit} from '@angular/core';
import {MediaState} from 'app/types/MediaState';
import {MediaEntityWorker} from 'app/types/MediaEntityWorker';

@Component({
  selector: 'app-download-item',
  templateUrl: './download-item.component.html',
  styleUrls: ['./download-item.component.scss']
})
export class DownloadItemComponent implements OnInit {

  @Input() mediaEntity: MediaEntityWorker;
  public errorMessage: string;

  constructor() {

  }

  ngOnInit(): void {
    this.mediaEntity.mediaService.onError.subscribe((exception) => {
      this.mediaEntity.entity.state = MediaState.Error;
      this.errorMessage = (exception.message || exception).toString();
    });
  }

  get MediaState(): typeof MediaState {
    return MediaState;
  }

}
