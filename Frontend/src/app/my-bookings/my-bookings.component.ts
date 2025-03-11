import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { AuthModalsComponent } from '../auth-modals/auth-modals.component';
import { AuthService } from '../services/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

// For /api/booking/customer
interface CustomerBookingResponse {
  bookingId: number;
  hotelId: number;
  hotelName: string;
  roomsBooked: {
    roomId: number;
    roomNumber: string;
    roomType: number;
    pricePerNight: number;
  }[];
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  totalPrice: number;
  status: number;
  specialRequest?: string;
}

// For /api/booking/{bookingId}
interface RawBooking {
  bookingId: number;
  hotelId: number;
  bookingRooms: {
    roomId: number;
    room: {
      id: number;
      roomNumber: string;
      type: number;
      pricePerNight: number;
    };
    checkInDate: string;
    checkOutDate: string;
  }[] | null;
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  totalPrice: number;
  status: number;
  specialRequest?: string;
}

interface Booking {
  bookingId: number;
  hotelId: number;
  hotelName: string;
  customerName: string | null;
  roomsBooked: {
    roomId: number;
    roomNumber: string;
    roomType: string;
    pricePerNight: number;
  }[];
  roomsBookedCount: number;
  checkInDate: string;
  checkOutDate: string;
  numberOfGuests: number;
  totalPrice: number;
  status: 'Pending' | 'Paid' | 'Cancelled';
  specialRequest?: string;
}

interface Payment {
  paymentId: number;
  bookingId: number;
  amountPaid: number;
  paymentMethod: string;
  paymentDate: string;
  paymentStatus: string;
  checkInDate: string;
  checkOutDate: string;
  roomPricePerNight: number;
}

interface Hotel {
  id: number;
  name: string;
  address: string;
  city: string;
  rating: number;
  amenities: string[];
}

interface Room {
  roomId: number;
  roomNumber: string;
  roomType: string;
  pricePerNight: number;
}

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [RouterModule, CommonModule, HeaderComponent, AuthModalsComponent],
  templateUrl: './my-bookings.component.html',
  styleUrls: ['./my-bookings.component.css']
})
export class MyBookingsComponent implements OnInit, OnDestroy {
  isModalOpen = false;
  userFullName: string | null = null;
  userInitial: string | null = null;
  private userSub: Subscription | undefined;

  activeTab = 'upcoming';
  upcomingBookings: (Booking & { payment?: Payment })[] = [];
  cancelledBookings: (Booking & { payment?: Payment })[] = [];
  completedBookings: (Booking & { payment?: Payment })[] = [];
  errorMessage: string | null = null;

  showPaymentOptions = false;
  selectedBookingId: number | null = null;
  paymentMethods = ['CreditCard', 'DebitCard', 'BankTransfer', 'UPI', 'PayPal'];
  paymentTimeout: any;

  showPaymentConfirmation = false;
  selectedPaymentMethod: string | null = null;
  selectedBooking: (Booking & { payment?: Payment }) | null = null;

  showBookingDetails = false;
  bookingDetails: any = null;

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userFullName = localStorage.getItem('userFullName');
    this.userInitial = this.userFullName ? this.userFullName.charAt(0).toUpperCase() : null;

    this.userSub = this.authService.user$.subscribe(user => {
      this.userFullName = user ? user.fullName : null;
      this.userInitial = this.userFullName ? this.userFullName.charAt(0).toUpperCase() : null;
    });

    console.log('Initializing MyBookingsComponent');
    this.fetchBookings();
  }

  private mapStatus(status: number): 'Pending' | 'Paid' | 'Cancelled' {
    const statusMap: { [key: number]: 'Pending' | 'Paid' | 'Cancelled' } = {
      0: 'Pending',
      1: 'Paid',
      2: 'Cancelled'
    };
    return statusMap[status] || 'Pending';
  }

  private mapRoomType(type: number): string {
    const roomTypeMap: { [key: number]: string } = {
      1: 'Single', // Adjusted to match backend enum (1-4)
      2: 'Double',
      3: 'Suite',
      4: 'StandardWithBalcony'
    };
    return roomTypeMap[type] || 'Unknown';
  }

  async fetchBookings(): Promise<void> {
    const token = this.authService.getToken() || sessionStorage.getItem('authToken');
    if (!token) {
      this.errorMessage = 'Authentication token not found. Please log in.';
      console.error('No token found');
      return;
    }

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    console.log('Fetching bookings with token:', token.slice(0, 20) + '...');

    try {
      const rawBookings = await this.http.get<CustomerBookingResponse[]>('http://localhost:5280/api/booking/customer', { headers }).toPromise();
      console.log('Raw bookings from API:', JSON.stringify(rawBookings, null, 2));

      if (!rawBookings || rawBookings.length === 0) {
        this.upcomingBookings = [];
        this.cancelledBookings = [];
        this.completedBookings = [];
        this.errorMessage = 'No bookings found.';
        console.log('No bookings returned from API');
        return;
      }

      const bookings: Booking[] = rawBookings.map(b => ({
        bookingId: b.bookingId,
        hotelId: b.hotelId,
        hotelName: b.hotelName || 'Unknown Hotel',
        customerName: null,
        roomsBooked: b.roomsBooked ? b.roomsBooked.map(r => ({
          roomId: r.roomId,
          roomNumber: r.roomNumber,
          roomType: this.mapRoomType(r.roomType),
          pricePerNight: r.pricePerNight
        })) : [],
        roomsBookedCount: b.roomsBooked ? b.roomsBooked.length : 0,
        checkInDate: b.checkInDate,
        checkOutDate: b.checkOutDate,
        numberOfGuests: b.numberOfGuests,
        totalPrice: b.totalPrice,
        status: this.mapStatus(b.status),
        specialRequest: b.specialRequest
      }));

      console.log('Mapped bookings:', JSON.stringify(bookings, null, 2));

      const payments = await this.http.get<Payment[]>('http://localhost:5280/api/payments/paid-bookings', { headers }).toPromise();
      console.log('Raw payments from API:', JSON.stringify(payments, null, 2));

      const enrichedBookings = bookings.map(booking => ({
        ...booking,
        payment: payments?.find(p => p.bookingId === booking.bookingId)
      }));

      console.log('Enriched bookings:', JSON.stringify(enrichedBookings, null, 2));

      this.upcomingBookings = enrichedBookings.filter(b => {
        const isUpcoming = b.status === 'Pending' || b.status === 'Paid';
        console.log(`Booking ${b.bookingId}: isUpcoming = ${isUpcoming}, status = ${b.status}`);
        return isUpcoming;
      });

      this.cancelledBookings = enrichedBookings.filter(b => {
        const isCancelled = b.status === 'Cancelled';
        console.log(`Booking ${b.bookingId}: isCancelled = ${isCancelled}, status = ${b.status}`);
        return isCancelled;
      });

      this.completedBookings = [];

      console.log('Upcoming Bookings:', JSON.stringify(this.upcomingBookings, null, 2));
      console.log('Cancelled Bookings:', JSON.stringify(this.cancelledBookings, null, 2));
      console.log('Completed Bookings:', JSON.stringify(this.completedBookings, null, 2));

      this.upcomingBookings.forEach(b => {
        if (b.status === 'Pending' && !b.payment) {
          console.log(`Starting timeout for booking ${b.bookingId}`);
          this.startPaymentTimeout(b.bookingId);
        }
      });

      this.errorMessage = null;
    } catch (error) {
      this.errorMessage = 'Failed to load bookings.';
      console.error('Fetch error:', error);
    }
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  onModalVisibilityChange(isOpen: boolean): void {
    this.isModalOpen = isOpen;
  }

  async viewDetails(bookingId: number): Promise<void> {
    const headers = this.getAuthHeaders();
    try {
      // Fetch booking details
      const booking = await this.http.get<RawBooking>(`http://localhost:5280/api/booking/${bookingId}`, { headers }).toPromise();
      console.log('Raw booking details from API:', JSON.stringify(booking, null, 2));
      if (!booking) {
        this.errorMessage = 'Booking not found.';
        return;
      }

      // Fetch hotel details
      const hotel = await this.http.get<Hotel>(`http://localhost:5280/api/hotels/${booking.hotelId}`, { headers }).toPromise();
      if (!hotel) {
        this.errorMessage = 'Hotel not found for this booking.';
        return;
      }

      // Get customer details from AuthService
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) {
        // Fallback: Try to get user details directly from session storage
        const fullName = sessionStorage.getItem('userFullName') || 'Unknown User';
        const email = sessionStorage.getItem('userEmail') || 'N/A';

        if (!fullName || !email) {
          this.errorMessage = 'User details not found. Please log in.';
          return;
        }

        // Map rooms from bookingRooms
        const rooms: Room[] = booking.bookingRooms ? booking.bookingRooms.map(br => ({
          roomId: br.roomId,
          roomNumber: br.room.roomNumber,
          roomType: this.mapRoomType(br.room.type),
          pricePerNight: br.room.pricePerNight
        })) : [];

        // Calculate number of days
        const checkIn = new Date(booking.checkInDate);
        const checkOut = new Date(booking.checkOutDate);
        const diffTime = Math.abs(checkOut.getTime() - checkIn.getTime());
        const numberOfDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        // Calculate bill
        const basePrice = rooms.reduce((sum, room) => sum + room.pricePerNight, 0) * numberOfDays;
        const taxRate = 0.18; // 18% tax (GST in India)
        const taxAmount = basePrice * taxRate;
        const totalAmount = basePrice + taxAmount;

        this.bookingDetails = {
          bookingId: booking.bookingId,
          customer: {
            name: fullName,
            email: email
          },
          hotel: {
            name: hotel.name,
            address: hotel.address,
            city: hotel.city,
            rating: hotel.rating || 0,
            amenities: hotel.amenities || []
          },
          rooms,
          checkInDate: booking.checkInDate,
          checkOutDate: booking.checkOutDate,
          numberOfGuests: booking.numberOfGuests,
          numberOfDays,
          bill: {
            basePrice,
            taxAmount,
            totalAmount
          },
          refundPolicy: `
            **Refund Policy**
            - Cancellations made 30 days or more before check-in: Full refund.
            - Cancellations made 15-29 days before check-in: 50% refund.
            - Cancellations made less than 15 days before check-in: Non-refundable.
            - Refunds are processed within 5-7 business days.
            - No refunds for no-shows or early departures.
            - Contact the hotel directly for special circumstances.
          `
        };
      } else {
        // Map rooms from bookingRooms
        const rooms: Room[] = booking.bookingRooms ? booking.bookingRooms.map(br => ({
          roomId: br.roomId,
          roomNumber: br.room.roomNumber,
          roomType: this.mapRoomType(br.room.type),
          pricePerNight: br.room.pricePerNight
        })) : [];

        // Calculate number of days
        const checkIn = new Date(booking.checkInDate);
        const checkOut = new Date(booking.checkOutDate);
        const diffTime = Math.abs(checkOut.getTime() - checkIn.getTime());
        const numberOfDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        // Calculate bill
        const basePrice = rooms.reduce((sum, room) => sum + room.pricePerNight, 0) * numberOfDays;
        const taxRate = 0.18; // 18% tax (GST in India)
        const taxAmount = basePrice * taxRate;
        const totalAmount = basePrice + taxAmount;

        this.bookingDetails = {
          bookingId: booking.bookingId,
          customer: {
            name: currentUser.fullName || 'Unknown User',
            email: currentUser.email || ''
          },
          hotel: {
            name: hotel.name,
            address: hotel.address,
            city: hotel.city,
            rating: hotel.rating || 0,
            amenities: hotel.amenities || []
          },
          rooms,
          checkInDate: booking.checkInDate,
          checkOutDate: booking.checkOutDate,
          numberOfGuests: booking.numberOfGuests,
          numberOfDays,
          bill: {
            basePrice,
            taxAmount,
            totalAmount
          },
          refundPolicy: `
            **Refund Policy**
            - Cancellations made 30 days or more before check-in: Full refund.
            - Cancellations made 15-29 days before check-in: 50% refund.
            - Cancellations made less than 15 days before check-in: Non-refundable.
            - Refunds are processed within 5-7 business days.
            - No refunds for no-shows or early departures.
            - Contact the hotel directly for special circumstances.
          `
        };
      }

      this.showBookingDetails = true;
    } catch (error) {
      this.errorMessage = 'Failed to load booking details. Please check if the server is running or if the data exists.';
      console.error('Error in viewDetails:', error);
    }
  }

  closeBookingDetails(): void {
    this.showBookingDetails = false;
    this.bookingDetails = null;
  }

  downloadPDF(): void {
    const element = document.getElementById('booking-details-content');
    if (!element) return;

    html2canvas(element, { scale: 2 }).then(canvas => {
      const imgData = canvas.toDataURL('image/png');
      const pdf = new jsPDF('p', 'mm', 'a4');
      const imgProps = pdf.getImageProperties(imgData);
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;

      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
      pdf.save(`Booking_Details_${this.bookingDetails.bookingId}.pdf`);
    });
  }

  cancelBooking(bookingId: number): void {
    const headers = this.getAuthHeaders();
    this.http.delete(`http://localhost:5280/api/booking/${bookingId}`, { headers }).subscribe({
      next: () => this.fetchBookings(),
      error: (error) => {
        this.errorMessage = 'Failed to cancel booking.';
        console.error(error);
      }
    });
  }

  showPaymentModal(bookingId: number): void {
    this.selectedBookingId = bookingId;
    this.selectedBooking = this.upcomingBookings.find(b => b.bookingId === bookingId) || null;
    this.showPaymentOptions = true;
  }

  closePaymentModal(): void {
    this.showPaymentOptions = false;
    this.selectedBookingId = null;
    this.selectedBooking = null;
  }

  selectPaymentMethod(method: string): void {
    this.selectedPaymentMethod = method;
    this.showPaymentOptions = false;
    this.showPaymentConfirmation = true;
  }

  closePaymentConfirmation(): void {
    this.showPaymentConfirmation = false;
    this.selectedPaymentMethod = null;
    this.selectedBooking = null;
    this.selectedBookingId = null;
  }

  confirmPayment(): void {
    if (!this.selectedBookingId || !this.selectedPaymentMethod) return;

    const paymentData = {
      bookingId: this.selectedBookingId,
      paymentMethod: this.selectedPaymentMethod
    };

    const headers = this.getAuthHeaders();
    this.http.post('http://localhost:5280/api/payments/process-payment', paymentData, { headers }).subscribe({
      next: (response) => {
        console.log('Payment processed:', response);
        this.closePaymentConfirmation();
        this.fetchBookings();
        clearTimeout(this.paymentTimeout);
      },
      error: (error) => {
        this.errorMessage = 'Payment failed. Please try again.';
        console.error(error);
        this.closePaymentConfirmation();
      }
    });
  }

  private startPaymentTimeout(bookingId: number): void {
    this.paymentTimeout = setTimeout(() => {
      const booking = this.upcomingBookings.find(b => b.bookingId === bookingId);
      if (booking && !booking.payment) {
        this.cancelPendingBooking(bookingId);
      }
    }, 60000); // 1 minute
  }

  private cancelPendingBooking(bookingId: number): void {
    const headers = this.getAuthHeaders();
    this.http.delete(`http://localhost:5280/api/booking/${bookingId}`, { headers }).subscribe({
      next: () => {
        this.fetchBookings();
        this.errorMessage = 'Payment not completed within 1 minute. Booking cancelled.';
      },
      error: (error) => console.error('Cancel Error:', error)
    });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken() || sessionStorage.getItem('authToken');
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  ngOnDestroy(): void {
    if (this.userSub) this.userSub.unsubscribe();
    clearTimeout(this.paymentTimeout);
  }
}