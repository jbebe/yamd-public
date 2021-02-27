import {EventEmitter, Injectable} from '@angular/core';
import {MediaEntity} from 'app/types/MediaEntity';
import {getUtcNowString} from 'app/utils/date';
import {getMediaType} from 'app/utils/url';
import {MediaState} from 'app/types/MediaState';
import {ApiService} from '../api-service/api.service';
import {SubmitResult} from 'app/types/SubmitResult';

@Injectable({
  providedIn: 'root'
})
export class MediaService {

  public onMediaReady: EventEmitter<MediaEntity> = new EventEmitter<MediaEntity>();
  public onHealthChange: EventEmitter<boolean> = new EventEmitter<boolean>();
  public onError: EventEmitter<any> = new EventEmitter<any>();
  public apiService: ApiService = new ApiService();

  constructor() { }

  public getDefaultMediaEntity(inputParts: URL, mediaId: string): MediaEntity {
    const newEntity: MediaEntity = {
      createdDate: getUtcNowString(),
      id: mediaId,
      mediaUrl: inputParts.href,
      state: MediaState.Created,
      type: getMediaType(inputParts),
      imageB64: 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
    };

    return newEntity;
  }

  public async submitUrlAsync(newEntity: MediaEntity): Promise<SubmitResult> {
    try {
      const response = await this.apiService.submitMediaAsync(newEntity);

      if (response.entity){
        this.onMediaReady.emit(response.entity);
        return;
      }

      await this.pollEntityAsync(response.token);
    }
    catch (exception){
      console.log(exception);
      this.onError.emit(exception);
    }
  }

  private async pollEntityAsync(token: string): Promise<void> {
    const startSec = 1;
    const endSec = 5;
    let speed =  startSec;
    let iteration = 20;

    const getEntityAsync = async () => {
      const xml = await this.apiService.getTokenResponseAsync(token);
      const entity = MediaEntity.fromStorageXml(xml);

      return entity;
    };

    return await new Promise(async (resolve, reject) => {
      const checkEntityAsync = async () => {
        console.log(`'polling speed: ${speed} sec, #${iteration}`);
        const entity = await getEntityAsync();
        if (entity.state === MediaState.Success || entity.state === MediaState.Error){
          this.onMediaReady.emit(entity);
          resolve();
          return;
        }

        speed = Math.min(endSec, speed + 1);
        if (--iteration === 0){
          reject();
          return;
        }

        setTimeout(checkEntityAsync, speed * 1000);
      };

      await checkEntityAsync();
    });
  }
}
