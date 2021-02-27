import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {DownloadComponent} from './components/pages/download/download.component';

const routes: Routes = [
  {
    path: '',
    component: DownloadComponent,
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
