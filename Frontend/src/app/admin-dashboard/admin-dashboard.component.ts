import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Subscription } from 'rxjs';
import { AuthModalsComponent } from '../auth-modals/auth-modals.component';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../environments/environment';
import Chart from 'chart.js/auto';

interface Hotel {
  id: number;
  name: string;
  city: string;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    AuthModalsComponent
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  isModalOpen: boolean = false;
  userFullName: string | null = null;
  userInitial: string | null = null;
  showUserDropdown: boolean = false;
  private userSub: Subscription | undefined;

  hotels: Hotel[] = [];
  hotelCountsByCity: { [key: string]: number } = {};
  cityKeys: string[] = [];
  private chartInstance: Chart | null = null;
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.userFullName = sessionStorage.getItem('userFullName');
    this.userInitial = this.userFullName ? this.userFullName.charAt(0).toUpperCase() : null;

    this.userSub = this.authService.user$.subscribe(user => {
      this.userFullName = user ? user.fullName : null;
      this.userInitial = this.userFullName ? this.userFullName.charAt(0).toUpperCase() : null;
    });

    this.fetchHotels();
  }

  fetchHotels(): void {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${sessionStorage.getItem('authToken') || ''}`
    });
    this.http.get<Hotel[]>(`${environment.apiUrl}/hotels/all`, { headers }).subscribe({
      next: (hotels) => {
        this.hotels = hotels;
        this.calculateHotelCountsByCity();
        this.cityKeys = Object.keys(this.hotelCountsByCity);
        this.renderBarChart();
      },
      error: (error) => {
        console.error('Error fetching hotels:', error);
        this.errorMessage = 'Failed to load hotels. Please try again later.';
      }
    });
  }

  calculateHotelCountsByCity(): void {
    this.hotelCountsByCity = {};
    this.hotels.forEach(hotel => {
      this.hotelCountsByCity[hotel.city] = (this.hotelCountsByCity[hotel.city] || 0) + 1;
    });
    this.cityKeys = Object.keys(this.hotelCountsByCity);
  }

  renderBarChart(): void {
    const ctx = document.getElementById('hotelCityBarChart') as HTMLCanvasElement;
    if (ctx) {
      if (this.chartInstance) {
        this.chartInstance.destroy();
      }
      console.log('Rendering bar chart with data:', this.hotelCountsByCity);
      this.chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: this.cityKeys,
          datasets: [{
            label: 'Number of Hotels',
            data: Object.values(this.hotelCountsByCity),
            backgroundColor: [
              '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', 
              '#9966FF', '#FF9F40', '#FF6B6B', '#4CAF50'
            ], // Multiple colors for bars
            borderColor: [
              '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', 
              '#9966FF', '#FF9F40', '#FF6B6B', '#4CAF50'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'Number of Hotels',
                font: {
                  size: 16 // Increased font size for y-axis title
                }
              },
              ticks: {
                stepSize: 1,
                font: {
                  size: 14 // Increased font size for y-axis labels
                }
              },
              grid: {
                color: '#e0e0e0', // Light gray grid lines
                lineWidth: 1
              }
            },
            x: {
              title: {
                display: true,
                text: 'Cities',
                font: {
                  size: 16 // Increased font size for x-axis title
                }
              },
              ticks: {
                maxRotation: 45,
                minRotation: 45,
                autoSkip: false,
                font: {
                  size: 14 // Increased font size for x-axis labels (city names)
                }
              },
              grid: {
                display: false // Hide vertical grid lines for cleaner look
              }
            }
          },
          plugins: {
            legend: {
              display: false
            },
            tooltip: {
              enabled: true, // Ensure tooltips are enabled
              backgroundColor: 'rgba(0, 0, 0, 0.8)', // Dark background for tooltips
              titleFont: {
                size: 14
              },
              bodyFont: {
                size: 12
              },
              padding: 10,
              cornerRadius: 4
            }
          }
        }
      });
    } else {
      console.error('Canvas element #hotelCityBarChart not found');
    }
  }

  toggleUserDropdown(): void {
    this.showUserDropdown = !this.showUserDropdown;
  }

  onModalVisibilityChange(isOpen: boolean): void {
    this.isModalOpen = isOpen;
  }

  logout(): void {
    this.authService.logout();
    this.showUserDropdown = false;
  }

  ngOnDestroy(): void {
    if (this.userSub) {
      this.userSub.unsubscribe();
    }
    if (this.chartInstance) {
      this.chartInstance.destroy();
    }
  }
}