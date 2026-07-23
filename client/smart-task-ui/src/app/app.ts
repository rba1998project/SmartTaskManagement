import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private router = inject(Router);
  private titleService = inject(Title);

  private readonly defaultTitle = 'Smart Task Management';

  ngOnInit(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const route = this.router.routerState.snapshot.root;
      const data = route.firstChild?.data;
      const title = (data as any)?.['title'];
      this.titleService.setTitle(title || this.defaultTitle);
    });
  }
}