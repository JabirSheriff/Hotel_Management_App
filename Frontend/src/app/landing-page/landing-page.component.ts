import { Component, OnInit, AfterViewInit, ViewChildren, QueryList, ElementRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HeaderComponent } from '../header/header.component';
import { AuthModalsComponent } from '../auth-modals/auth-modals.component';
import { SearchService } from '../services/search.service';
import { HotelDataService } from '../services/hotel-data.service';
import { Hotel } from '../models/hotel';
import { trigger, state, style, animate, transition } from '@angular/animations';

// Define an interface for Offer
interface Offer {
  bank: string;
  title: string;
  description: string;
  image: string;
  buttonText: string;
}

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HeaderComponent, AuthModalsComponent],
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css'],
  animations: [
    trigger('fadeOut', [
      state('in', style({ opacity: 1, transform: 'translateY(0)' })),
      state('out', style({ opacity: 0, transform: 'translateY(-20px)' })),
      transition('in => out', animate('500ms ease-in-out')),
      transition('out => in', animate('500ms ease-in-out'))
    ])
  ]
})
export class LandingPageComponent implements OnInit, AfterViewInit, OnDestroy {
  isModalOpen: boolean = false;
  isLoading: boolean = false;
  isTransitioning: boolean = false;
  hotels: Hotel[] = [];
  searchLocation: string = '';
  checkInDate: string = '';
  checkOutDate: string = '';
  checkOutMinDate: string = '';
  rooms: number = 1;
  adults: number = 2;
  kids: number = 0;
  showGuestsDropdown: boolean = false;
  locationSuggestions: string[] = [];
  showLocationDropdown: boolean = false;
  allCities: string[] = [];
  today: string = '';

  // Existing cities with icons (for the top bar)
  cities = [
    { name: 'Bengaluru', icon: 'bangalore.png' },
    { name: 'Chennai', icon: 'chennai.png' },
    { name: 'Mumbai', icon: 'mumbai.png' },
    { name: 'Hyderabad', icon: 'hyderabad.png' },
    { name: 'Kolkata', icon: 'kolkata.png' },
    { name: 'Bhubaneswar', icon: 'odisha.png' },
  ];

  // Data for "Popular with travelers from India" section
  popularCities = [
    'Agartala', 'Aizawl', 'Amaravati', 'Bengaluru', 'Bhopal', 'Bhubaneswar', 'Chandigarh',
    'Chennai', 'Dehradun', 'Dispur', 'Gandhinagar', 'Gangtok', 'Hyderabad', 'Imphal',
    'Itanagar', 'Jaipur', 'Kohima', 'Kolkata', 'Lucknow', 'Mumbai', 'Panaji', 'Patna',
    'Raipur', 'Ranchi', 'Shillong', 'Shimla', 'Thiruvananthapuram'
  ];

  internationalCities = [
    'Dubai', 'Singapore', 'Bangkok', 'London', 'New York', 'Paris', 'Tokyo', 'Sydney',
    'Dubai', 'Kuala Lumpur', 'Istanbul', 'Amsterdam', 'Rome', 'Toronto', 'Melbourne'
  ];

  regions = [
    'Himalayas', 'Western Ghats', 'Rajasthan Desert', 'Kerala Backwaters', 'Northeast India',
    'Goa Beaches', 'Darjeeling Hills', 'Andaman Islands', 'Ladakh', 'Coastal Karnataka'
  ];

  countries = [
    'United Arab Emirates', 'Thailand', 'Singapore', 'United Kingdom', 'United States',
    'France', 'Japan', 'Australia', 'Malaysia', 'Turkey'
  ];

  placesToStay = [
    'Luxury Resorts', 'Budget Hotels', 'Homestays', 'Villas', 'Hostels', 'Beach Resorts',
    'Mountain Lodges', 'Serviced Apartments', 'Guest Houses', 'Eco-Friendly Stays'
  ];

  selectedCityIndex: number | null = null;
  cityPositions: number[] = [];
  underlineWidth: number = 0;
  underlinePosition: number = 0;
  @ViewChildren('cityButton') cityButtons!: QueryList<ElementRef<HTMLButtonElement>>;

  pricePerNightRanges: string[] = ['Any', '0-1500', '1500-2500', '2500-5000', '5000+'];
  selectedPricePerNight: string = 'Any';
  showPricePerNightDropdown: boolean = false;

  // Tabs and offers
  tabs: string[] = ['ON 1st BOOKING', 'Hotels', 'Holidays', 'All Offers', 'Bank Offers'];
  selectedTab: string = 'ON 1st BOOKING';
  currentOffset: number = 0;
  cardWidth: number = 400; // Wider cards (image + details side by side)

  // Popular section tabs
  popularTabs: string[] = ['Domestic cities', 'International cities', 'Regions', 'Countries', 'Places to stay'];
  selectedPopularTab: string = 'Domestic cities';

  // Offer data for each tab (10+ offers per tab)
  allOffers: { [key: string]: Offer[] } = {
    'ON 1st BOOKING': [
      { bank: 'AMEX', title: 'Grab FLAT 12% OFF* on Flights', description: 'with American Express Cards.', image: 'offer1.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: 'Interest-free EMI* + 35% OFF', description: 'on flights & hotels.', image: 'offer2.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '15% OFF on Next Trip!', description: 'on hotels & homestays.', image: 'offer3.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '25% OFF on Holidays', description: 'worldwide packages.', image: 'offer4.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '#5L OFF on Flights', description: 'with EMI options.', image: 'offer5.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '10% OFF on Hotels', description: 'exclusive deal.', image: 'offer6.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '20% OFF Summer Sale', description: 'on holiday packages.', image: 'offer7.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '15% OFF on Flights', description: 'international deals.', image: 'offer8.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '30% OFF on Stays', description: 'longer bookings.', image: 'offer9.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: 'Free Breakfast Deal', description: '10% OFF weekends.', image: 'offer10.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '40% OFF on Trains', description: 'special offer.', image: 'offer11.png', buttonText: 'Book Now' }
    ],
    'Hotels': [
      { bank: 'AMEX', title: '20% OFF Hotel Stays', description: 'with Amex cards.', image: 'offer1.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '30% OFF Luxury Hotels', description: 'in India.', image: 'offer2.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '15% OFF Resorts', description: 'weekend getaways.', image: 'offer3.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '25% OFF Budget Stays', description: 'EMI available.', image: 'offer4.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '10% OFF Suites', description: 'exclusive offer.', image: 'offer5.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: 'Free Upgrade', description: 'on hotel bookings.', image: 'offer6.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '20% OFF Spa Hotels', description: 'relax and save.', image: 'offer7.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '35% OFF Family Stays', description: 'special deal.', image: 'offer8.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '15% OFF City Hotels', description: 'urban escapes.', image: 'offer9.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '25% OFF Beach Resorts', description: 'summer special.', image: 'offer10.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '10% OFF Inns', description: 'cozy stays.', image: 'offer11.png', buttonText: 'Book Now' }
    ],
    'Holidays': [
      { bank: 'AMEX', title: '30% OFF Holiday Packages', description: 'worldwide destinations.', image: 'offer1.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '25% OFF Beach Holidays', description: 'summer getaways.', image: 'offer2.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '20% OFF Mountain Trips', description: 'adventure awaits.', image: 'offer3.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '15% OFF Family Holidays', description: 'EMI options available.', image: 'offer4.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '10% OFF Cruise Packages', description: 'luxury on the sea.', image: 'offer5.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '35% OFF Winter Holidays', description: 'snowy escapes.', image: 'offer6.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '40% OFF Cultural Tours', description: 'explore heritage.', image: 'offer7.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '20% OFF Adventure Holidays', description: 'thrilling experiences.', image: 'offer8.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '25% OFF Safari Trips', description: 'wildlife adventures.', image: 'offer9.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '30% OFF City Breaks', description: 'urban holidays.', image: 'offer10.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '15% OFF Island Getaways', description: 'tropical paradise.', image: 'offer11.png', buttonText: 'Book Now' }
    ],
    'All Offers': [
      { bank: 'AMEX', title: '50% OFF All Trips', description: 'limited time.', image: 'offer1.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '40% OFF Everything', description: 'travel deals.', image: 'offer2.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '30% OFF All Bookings', description: 'flights & more.', image: 'offer3.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '20% OFF All Services', description: 'EMI options.', image: 'offer4.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '15% OFF All Hotels', description: 'exclusive deal.', image: 'offer5.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '25% OFF All Flights', description: 'worldwide.', image: 'offer6.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '35% OFF All Packages', description: 'holiday deals.', image: 'offer7.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '10% OFF All Trips', description: 'special offer.', image: 'offer8.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '20% OFF All Stays', description: 'hotel deals.', image: 'offer9.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '30% OFF All Travel', description: 'global offers.', image: 'offer10.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '15% OFF All Services', description: 'wide range.', image: 'offer11.png', buttonText: 'Book Now' }
    ],
    'Bank Offers': [
      { bank: 'AMEX', title: '10% Cashback', description: 'on all cards.', image: 'offer1.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '20% Cashback', description: 'on travel.', image: 'offer2.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '15% Cashback', description: 'on bookings.', image: 'offer3.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '25% Cashback', description: 'with EMI.', image: 'offer4.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '5% Cashback', description: 'on hotels.', image: 'offer5.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '30% Cashback', description: 'on flights.', image: 'offer6.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '10% Cashback', description: 'on packages.', image: 'offer7.png', buttonText: 'Book Now' },
      { bank: 'BAJAJ FINSERV', title: '20% Cashback', description: 'on stays.', image: 'offer8.png', buttonText: 'Book Now' },
      { bank: 'AMEX', title: '15% Cashback', description: 'on travel.', image: 'offer9.png', buttonText: 'View Details' },
      { bank: 'YES BANK', title: '25% Cashback', description: 'on holidays.', image: 'offer10.png', buttonText: 'Book Now' },
      { bank: 'KOTAK BANK', title: '5% Cashback', description: 'on all.', image: 'offer11.png', buttonText: 'Book Now' }
    ]
  };

  currentOffers: Offer[] = [];

  constructor(
    private router: Router,
    private http: HttpClient,
    private searchService: SearchService,
    private hotelDataService: HotelDataService
  ) {}

  ngOnInit(): void {
    const today = new Date();
    this.today = today.toISOString().split('T')[0];
    this.checkInDate = new Date(today.setDate(today.getDate() + 1)).toISOString().split('T')[0];
    this.checkOutDate = new Date(today.setDate(today.getDate() + 8)).toISOString().split('T')[0];
    this.updateCheckOutMinDate();
    this.isTransitioning = false;
    this.fetchAllCities();
    this.selectTab(this.selectedTab); // Load initial offers
  }

  ngAfterViewInit(): void {
    this.calculateCityPositions();
    this.cityButtons.changes.subscribe(() => this.calculateCityPositions());
    if (this.selectedCityIndex !== null) this.updateUnderline();
  }

  ngOnDestroy(): void {}

  onModalVisibilityChange(isOpen: boolean): void {
    this.isModalOpen = isOpen;
  }

  calculateCityPositions(): void {
    const buttons = this.cityButtons.toArray();
    this.cityPositions = buttons.map(() => 0);
    if (this.selectedCityIndex !== null) this.updateUnderline();
  }

  updateUnderline(): void {
    if (this.selectedCityIndex !== null) {
      const buttons = this.cityButtons.toArray();
      this.underlineWidth = buttons[this.selectedCityIndex]?.nativeElement.offsetWidth || 0;
      this.underlinePosition = 0;
    } else {
      this.underlineWidth = 0;
      this.underlinePosition = 0;
    }
  }

  updateCheckOutMinDate(): void {
    if (this.checkInDate) {
      const checkIn = new Date(this.checkInDate);
      checkIn.setDate(checkIn.getDate() + 1);
      this.checkOutMinDate = checkIn.toISOString().split('T')[0];
      if (new Date(this.checkOutDate) <= new Date(this.checkInDate)) {
        this.checkOutDate = this.checkOutMinDate;
      }
    }
  }

  toggleGuestsDropdown(): void {
    this.showGuestsDropdown = !this.showGuestsDropdown;
  }

  updateRooms(change: number): void {
    const newRooms = this.rooms + change;
    if (newRooms >= 1) this.rooms = newRooms;
  }

  updateAdults(change: number): void {
    const newAdults = this.adults + change;
    if (newAdults >= 1) this.adults = newAdults;
  }

  updateKids(change: number): void {
    const newKids = this.kids + change;
    if (newKids >= 0) this.kids = newKids;
  }

  fetchAllCities(): void {
    this.http.get<string[]>('http://localhost:5280/api/hotels/cities')
      .subscribe({
        next: (cities) => {
          this.allCities = cities;
          this.locationSuggestions = cities;
        },
        error: (error) => {
          console.error('Error fetching cities:', error);
          this.allCities = ['Fallback City 1', 'Fallback City 2'];
          this.locationSuggestions = this.allCities;
        }
      });
  }

  onLocationFocus(): void {
    this.showLocationDropdown = true;
    this.locationSuggestions = this.allCities;
  }

  onLocationInput(event: Event): void {
    const input = (event.target as HTMLInputElement).value;
    this.showLocationDropdown = true;

    if (input.length > 0) {
      this.http.get<string[]>(`http://localhost:5280/api/hotels/cities?q=${input}`)
        .subscribe({
          next: (cities) => {
            this.locationSuggestions = cities;
          },
          error: (error) => {
            console.error('Error filtering cities:', error);
            this.locationSuggestions = [];
          }
        });
    } else {
      this.locationSuggestions = this.allCities;
    }
  }

  selectLocation(suggestion: string): void {
    this.searchLocation = suggestion;
    this.showLocationDropdown = false;
    this.locationSuggestions = this.allCities;
  }

  hideLocationDropdown(): void {
    setTimeout(() => {
      this.showLocationDropdown = false;
    }, 200);
  }

  selectCity(index: number): void {
    this.selectedCityIndex = index;
    this.calculateCityPositions();
    this.updateUnderline();
    this.selectLocation(this.cities[index].name);
  }

  togglePricePerNightDropdown(): void {
    this.showPricePerNightDropdown = !this.showPricePerNightDropdown;
  }

  selectPricePerNight(pricePerNight: string): void {
    this.selectedPricePerNight = pricePerNight;
    this.showPricePerNightDropdown = false;
  }

  onSearch(): void {
    this.isLoading = true;
    let params = new HttpParams();

    if (this.searchLocation) params = params.set('searchTerm', this.searchLocation);
    if (this.checkInDate) params = params.set('checkInDate', this.checkInDate);
    if (this.checkOutDate) params = params.set('checkOutDate', this.checkOutDate);
    params = params.set('numberOfGuests', (this.adults + this.kids).toString());
    params = params.set('numberOfRooms', this.rooms.toString());

    const maxPrice = this.selectedPricePerNight === 'Any' ? null : this.getPricePerNightValue(this.selectedPricePerNight, 'max');
    if (maxPrice !== null) params = params.set('maxPrice', maxPrice.toString());

    this.http.get<Hotel[]>('http://localhost:5280/api/hotels/search', { params })
      .subscribe({
        next: (hotels) => {
          this.hotels = hotels.map(hotel => ({
            ...hotel,
            imageUrl: hotel.images.find(img => img.isPrimary)?.imageUrl || 'https://via.placeholder.com/300x200',
            location: `${hotel.city}, ${hotel.country}`,
            pricePerNight: Math.min(...hotel.rooms.map(room => room.pricePerNight)),
            originalPrice: Math.min(...hotel.rooms.map(room => room.pricePerNight)) * 1.5,
            rating: 4.2,
            ratingCount: 803,
            description: hotel.description || 'Conveniently located near key landmarks, clean rooms, tasty food',
            currentImageIndex: 0
          }));
          this.isLoading = false;
          this.isTransitioning = true;
          setTimeout(() => {
            this.hotelDataService.setHotels(this.hotels);
            this.searchService.setSearchParams({
              searchLocation: this.searchLocation,
              checkInDate: this.checkInDate,
              checkOutDate: this.checkOutDate,
              rooms: this.rooms,
              adults: this.adults,
              kids: this.kids,
              selectedPricePerNight: this.selectedPricePerNight
            });
            this.router.navigate(['/search']);
          }, 500);
        },
        error: (error) => {
          console.error('Error searching hotels:', error);
          this.isLoading = false;
          alert('Failed to load hotels. Please try again.');
        }
      });
  }

  private getPricePerNightValue(pricePerNight: string, type: 'max'): number | null {
    switch (pricePerNight) {
      case '0-1500': return 1500;
      case '1500-2500': return 2500;
      case '2500-5000': return 5000;
      case '5000+': return null;
      default: return null;
    }
  }

  onImageError(event: Event, name: string): void {
    console.error(`Failed to load image for ${name}:`, (event.target as HTMLImageElement).src);
    (event.target as HTMLImageElement).src = 'https://via.placeholder.com/300x200';
  }

  onImageLoad(event: Event, name: string): void {
    console.log(`Successfully loaded image for ${name}:`, (event.target as HTMLImageElement).src);
  }

  // Tab and carousel methods
  selectTab(tab: string): void {
    this.selectedTab = tab;
    this.currentOffers = this.allOffers[tab] || [];
    this.currentOffset = 0; // Reset offset when switching tabs
  }

  nextOffer(): void {
    const visibleCards = 4; // 2 rows x 2 columns
    const totalOffers = this.currentOffers.length;
    if (totalOffers <= visibleCards) return; // No need to slide if all offers are visible
    const maxOffset = -((totalOffers - visibleCards) * this.cardWidth);
    if (this.currentOffset > maxOffset) {
      this.currentOffset -= this.cardWidth * 2; // Move 2 cards at a time (1 set of 2 columns)
    }
  }

  prevOffer(): void {
    if (this.currentOffset < 0) {
      this.currentOffset += this.cardWidth * 2; // Move 2 cards back
    }
  }

  getOfferChunks(): number[] {
    const totalOffers = this.currentOffers.length;
    const chunks = Math.ceil(totalOffers / 4); // Each chunk is 4 cards (2 rows x 2 columns)
    return Array.from({ length: chunks }, (_, i) => i);
  }

  // Updated method for popular tab selection
  selectPopularTab(tab: string): void {
    this.selectedPopularTab = tab;
    // No additional logic needed yet; data is handled in the template
  }

  // Helper to append "hotels" or relevant suffix based on tab
  getItemLabel(item: string, tab: string): string {
    switch (tab) {
      case 'Domestic cities':
      case 'International cities':
        return `${item} hotels`;
      case 'Regions':
        return `${item} tours`;
      case 'Countries':
        return `${item} destinations`;
      case 'Places to stay':
        return `${item}`;
      default:
        return item;
    }
  }
}