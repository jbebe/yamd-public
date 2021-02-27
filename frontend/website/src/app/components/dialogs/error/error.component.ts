import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';
import {YamdException} from 'app/types/YamdException';

@Component({
  selector: 'app-error-dialog',
  templateUrl: 'error.component.html',
  styleUrls: ['./error.component.scss']
})
export class ErrorDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: YamdException) {}
}
