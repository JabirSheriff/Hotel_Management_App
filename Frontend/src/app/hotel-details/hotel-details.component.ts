import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { Hotel, HotelImage, Amenity, Room, Review } from '../models/hotel';
import { Observable, Subscription, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

interface BookingResponse {
  id: number;
  hotelId: number;
  customerId: number;
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  totalPrice: number;
  status: string; // 'Pending', 'Paid', 'Cancelled'
  specialRequest?: string;
  bookingRooms: { roomId: number }[];
}

@Component({
  selector: 'app-hotel-details',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './hotel-details.component.html',
  styleUrls: ['./hotel-details.component.css']
})
export class HotelDetailsComponent implements OnInit, OnDestroy {
  hotel: Hotel | undefined;
  reviews: Review[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  reviewForm: FormGroup;
  bookingForm: FormGroup;
  isAuthenticated = false;
  customerName = 'Anonymous';
  customerId: number | null = null;
  showBookingModal = false;
  showPaymentModal = false;
  bookedDetails: BookingResponse | null = null;
  roomTypes = [
    { value: 0, label: 'Standard With Balcony' },
    { value: 1, label: 'Superior With Balcony' },
    { value: 2, label: 'Premium With Balcony' }
  ];
  minCheckInDate: string = new Date().toISOString().split('T')[0];
  private authSub: Subscription | undefined;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private authService: AuthService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.reviewForm = this.fb.group({
      rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', [Validators.required, Validators.maxLength(500)]]
    });

    this.bookingForm = this.fb.group({
      checkInDate: ['', Validators.required],
      checkOutDate: ['', Validators.required],
      numberOfGuests: [1, [Validators.required, Validators.min(1), Validators.max(10)]],
      roomType: [0, Validators.required],
      specialRequest: ['']
    });
  }

  ngOnInit(): void {
    this.authSub = this.authService.user$.subscribe(user => {
      this.isAuthenticated = !!user;
      this.customerName = user?.fullName ?? 'Anonymous';
      this.customerId = this.getCustomerIdFromToken();
    });

    const hotelId = this.route.snapshot.paramMap.get('id');
    if (hotelId) {
      if (!this.isAuthenticated) {
        this.errorMessage = 'Please log in to view hotel details and book a room.';
        return;
      }
      this.fetchHotelDetails(+hotelId);
      this.fetchReviews(+hotelId);
    } else {
      this.errorMessage = 'Invalid hotel ID.';
    }
  }

  ngOnDestroy(): void {
    if (this.authSub) this.authSub.unsubscribe();
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return token ? new HttpHeaders().set('Authorization', `Bearer ${token}`) : new HttpHeaders();
  }

  private getCustomerIdFromToken(): number | null {
    const token = this.authService.getToken() || sessionStorage.getItem('token');
    if (!token) return null;

    try {
      const payloadBase64 = token.split('.')[1];
      const decodedPayload = atob(payloadBase64);
      const payload = JSON.parse(decodedPayload);
      return payload.customerId || payload.id || payload.sub || null;
    } catch (e) {
      console.error('Error decoding token:', e);
      return null;
    }
  }

  fetchHotelDetails(id: number): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.http.get<Hotel>(`http://localhost:5280/api/hotels/${id}`, { headers: this.getAuthHeaders() })
      .pipe(
        catchError(error => {
          console.error('Error fetching hotel details:', error);
          this.errorMessage = 'Failed to load hotel details. Please ensure you are logged in.';
          this.isLoading = false;
          return of(null);
        })
      )
      .subscribe(hotel => {
        if (hotel) {
          const latitude = 20 + (Math.random() * 20 - 10);
          const longitude = 70 + (Math.random() * 20 - 10);
          this.hotel = { ...hotel, currentImageIndex: 0, latitude, longitude };
        }
        this.isLoading = false;
      });
  }

  fetchReviews(hotelId: number): void {
    this.http.get<Review[]>(`http://localhost:5280/api/reviews/${hotelId}`, { headers: this.getAuthHeaders() })
      .pipe(
        catchError(error => {
          console.error('Error fetching reviews:', error);
          this.errorMessage = 'Failed to load reviews.';
          return of([]);
        })
      )
      .subscribe(reviews => this.reviews = reviews);
  }

  prevImage(): void {
    if (this.hotel && this.hotel.images && this.hotel.images.length > 0) {
      this.hotel.currentImageIndex = (this.hotel.currentImageIndex! - 1 + this.hotel.images.length) % this.hotel.images.length;
    }
  }

  nextImage(): void {
    if (this.hotel && this.hotel.images && this.hotel.images.length > 0) {
      this.hotel.currentImageIndex = (this.hotel.currentImageIndex! + 1) % this.hotel.images.length;
    }
  }

  getAverageRating(): number {
    return this.reviews.length ? this.reviews.reduce((sum, r) => sum + r.rating, 0) / this.reviews.length : 0;
  }

  getRoomTypeName(type: string | undefined): string {
    if (!type) return 'Unknown Room Type';
    const typeNum = parseInt(type);
    const roomTypes: { [key: number]: string } = {
      1: 'Standard With Balcony',
      2: 'Superior With Balcony',
      3: 'Premium With Balcony'
    };
    return roomTypes[typeNum] || 'Unknown Room Type';
  }

  onSubmitReview(): void {
    if (this.reviewForm.valid && this.hotel?.id) {
      const headers = this.getAuthHeaders();
      const reviewData = {
        hotelId: this.hotel.id,
        customerId: this.customerId,
        rating: this.reviewForm.value.rating,
        comment: this.reviewForm.value.comment
      };
      this.http.post<Review>('http://localhost:5280/api/reviews', reviewData, { headers }).subscribe({
        next: (newReview) => {
          this.reviews.push(newReview);
          this.reviewForm.reset({ rating: 0, comment: '' });
          this.errorMessage = null;
        },
        error: (error) => {
          this.errorMessage = `Failed to submit review: ${error.statusText || 'Unknown error'}.`;
          console.error(error);
        }
      });
    } else {
      this.errorMessage = 'Form is invalid or hotel ID is missing.';
    }
  }

  bookNow(): void {
    if (!this.isAuthenticated) {
      this.errorMessage = 'Please log in to book a room.';
      return;
    }
    this.showBookingModal = true;
    this.errorMessage = null;
  }

  closeBookingModal(): void {
    this.showBookingModal = false;
    this.bookingForm.reset({ roomType: 0, numberOfGuests: 1 });
    this.errorMessage = null;
  }

  onSubmitBooking(): void {
    if (!this.isAuthenticated) {
      this.errorMessage = 'Please log in to book a room.';
      return;
    }
    if (this.bookingForm.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      this.markFormGroupTouched(this.bookingForm);
      return;
    }

    const formValue = this.bookingForm.value;
    const checkInDate = new Date(formValue.checkInDate);
    const checkOutDate = new Date(formValue.checkOutDate);
    if (isNaN(checkInDate.getTime()) || isNaN(checkOutDate.getTime())) {
      this.errorMessage = 'Invalid date format. Please select valid dates.';
      return;
    }
    if (checkInDate >= checkOutDate) {
      this.errorMessage = 'Check-out date must be after check-in date.';
      return;
    }

    if (this.hotel?.id) {
      const customerId = this.getCustomerIdFromToken();
      if (!customerId) {
        this.errorMessage = 'Unable to book: Customer ID not found.';
        return;
      }

      const bookingData = {
        hotelId: this.hotel.id,
        roomType: formValue.roomType, // 0-2 maps to 1-3 in backend
        checkInDate: checkInDate.toISOString(),
        checkOutDate: checkOutDate.toISOString(),
        numberOfRooms: 1,
        numberOfGuests: formValue.numberOfGuests,
        specialRequest: formValue.specialRequest || ''
      };

      const headers = this.getAuthHeaders();
      this.http.post<BookingResponse>('http://localhost:5280/api/booking/add', bookingData, { headers }).subscribe({
        next: (response) => {
          this.bookedDetails = response;
          this.showBookingModal = false;
          this.showPaymentModal = true;
          this.errorMessage = null;
          this.cdr.detectChanges();
        },
        error: (error) => {
          const backendMessage = error.error || error.message || 'Unknown error';
          if (backendMessage.includes('No rooms of type')) {
            this.errorMessage = backendMessage;
          } else if (backendMessage.includes('Guest capacity')) {
            this.errorMessage = backendMessage;
          } else if (backendMessage.includes('No rooms available') || backendMessage.includes('Only')) {
            this.errorMessage = backendMessage;
          } else {
            this.errorMessage = `Failed to book: ${backendMessage}.`;
          }
          console.error('Booking Error:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.errorMessage = 'Unable to book: Hotel information is missing.';
    }
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.bookedDetails = null;
  }

  proceedToPayment(): void {
    if (this.bookedDetails?.id) {
      this.showPaymentModal = false;
      this.router.navigate(['/my-bookings']); // Redirect to My Bookings
    } else {
      this.errorMessage = 'No booking details available to proceed with payment.';
    }
  }

  getInitials(name: string): string {
    const names = name.split(' ');
    const initials = names.map(n => n.charAt(0).toUpperCase()).join('');
    return initials.length > 2 ? initials.substring(0, 2) : initials;
  }

  getAvatarColor(name: string): string {
    const colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEEAD', '#D4A5A5'];
    return colors[name.length % colors.length];
  }

  get minCheckOutDate(): string {
    const checkInDate = this.bookingForm.get('checkInDate')?.value as string | undefined;
    return checkInDate || this.minCheckInDate;
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) this.markFormGroupTouched(control);
    });
  }
}