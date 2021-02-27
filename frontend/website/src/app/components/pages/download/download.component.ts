import {AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild} from '@angular/core';
import {getUrlParts, normalizeUrl} from 'app/utils/url';
import {MediaEntity} from 'app/types/MediaEntity';
import {MediaState} from 'app/types/MediaState';
import {YamdException} from 'app/types/YamdException';
import {MatDialog} from '@angular/material/dialog';
import {ErrorDialogComponent} from '../../dialogs/error/error.component';
import {generateMediaId} from 'app/utils/random';
import {MediaService} from 'app/services/media-service/media.service';
import {FormControl} from '@angular/forms';
import {debounceTime, distinctUntilChanged, filter} from 'rxjs/operators';
import {fromEvent} from 'rxjs';
import {MediaEntityWorker} from 'app/types/MediaEntityWorker';

@Component({
  selector: 'app-download',
  templateUrl: './download.component.html',
  styleUrls: ['./download.component.scss']
})
export class DownloadComponent implements OnInit, AfterViewInit {

  urlInput = new FormControl('', []);
  inputError: string | null = null;
  @ViewChild('inputElement') inputElement;

  mediaHistory: Map<string, MediaEntityWorker> = new Map<string, MediaEntityWorker>();
  isApiOnline = true;

  get MediaState(): typeof MediaState {
    return MediaState;
  }

  constructor(
    public dialog: MatDialog,
    public mediaService: MediaService,
    public changeDetector: ChangeDetectorRef,
  ) {
  }

  // Hook events after component initialization
  ngOnInit(): void {
    this.mediaService.onHealthChange.subscribe(isApiOnline => {
      this.isApiOnline = isApiOnline;
    });
    this.mediaService.onError.subscribe(e => this.showError(e));
  }

  // Hook events after element in DOM
  ngAfterViewInit(): void {
    this.inputElement.nativeElement.focus();
    this.changeDetector.detectChanges();

    // Listen for input change
    fromEvent(this.inputElement.nativeElement, 'input')
      .pipe(
        filter(Boolean),
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(async (v) => {
        await this.onSubmitMediaUrlAsync();
      });
  }

  async onSubmitMediaUrlAsync(): Promise<void> {
    // Parse URL
    const inputParts = getUrlParts(this.urlInput.value);
    if (!inputParts) {
      this.inputError = 'Invalid url';
      this.urlInput.setErrors({'DUMMY': ''});
      return;
    }

    // Generate ID from URL and check for duplicate
    const mediaId = generateMediaId(normalizeUrl(inputParts));
    if (this.mediaHistory.has(mediaId)) {
      return; // avoid repeated events
    }

    try {
      const worker = new MediaEntityWorker(inputParts, mediaId, (w) => {
        this.mediaHistory.set(w.entity.id, w);
        this.updateMediaHistory();
      });

      this.mediaHistory.set(mediaId, worker);
      this.updateMediaHistory();

      this.urlInput.reset();
    }
    catch (ex) {
      console.log(ex);
      this.showError(ex);
    }
  }

  private updateMediaHistory(): void {
    this.mediaHistory = new Map<string, MediaEntityWorker>(this.mediaHistory);
  }

  private showError(exception: any): void {
    this.dialog.open(ErrorDialogComponent, {
      data: (exception as YamdException) ?? exception.toString(),
    });
  }
}
