import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IndexComponent } from './index/index.component';

// Component pages
 
const routes: Routes = [
  { path: '/', loadChildren: () => IndexComponent },
  { path: 'test', loadChildren: () => IndexComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PagesRoutingModule { }
 