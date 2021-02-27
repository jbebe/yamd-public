import {MediaEntity} from 'app/types/MediaEntity';
import {MediaService} from 'app/services/media-service/media.service';

export class MediaEntityWorker {

  public entity: MediaEntity;
  public mediaService: MediaService = new MediaService();

  constructor(url: URL, mediaId: string, onMediaReady: (_: MediaEntityWorker) => void) {
    this.entity = this.mediaService.getDefaultMediaEntity(url, mediaId);

    // Enqueue download link generation request
    this.mediaService.onMediaReady.subscribe((mediaEntity: MediaEntity) => {
      this.entity = mediaEntity;
      onMediaReady(this);
    });

    // noinspection ES6MissingAwait
    // noinspection JSIgnoredPromiseFromCall
    this.mediaService.submitUrlAsync(this.entity);
  }

}
