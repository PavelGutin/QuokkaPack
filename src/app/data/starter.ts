/**
 * A sensible starting catalog offered on first run. Generic on purpose: no brands or people. Each category doubles as
 * a trip "template" on the New trip screen (a work trip picks Work, a beach week picks Beach and Kids).
 */
export const STARTER_CATALOG: Record<string, string[]> = {
  Clothing: ['T-shirts', 'Underwear', 'Socks', 'Pants', 'Shorts', 'Sweater', 'Jacket', 'Rain jacket', 'Pajamas', 'Shoes', 'Sandals', 'Hat', 'Belt'],
  Toiletries: ['Toothbrush', 'Toothpaste', 'Floss', 'Deodorant', 'Shampoo', 'Conditioner', 'Body wash', 'Razor', 'Sunscreen', 'Lip balm', 'Hairbrush', 'Bug spray'],
  Electronics: ['Phone charger', 'Headphones', 'Power bank', 'Laptop', 'Laptop charger', 'Travel adapter', 'USB cables', 'E-reader'],
  Documents: ['Passport', 'ID / driver’s license', 'Wallet', 'Reservations and confirmations', 'Travel insurance', 'Boarding pass'],
  Health: ['Medications', 'First-aid kit', 'Pain relievers', 'Allergy medicine', 'Glasses / contacts'],
  Essentials: ['Keys', 'Water bottle', 'Sunglasses', 'Snacks', 'Day bag', 'Earplugs', 'Umbrella'],
  Beach: ['Swimsuit', 'Beach towel', 'Flip-flops', 'Beach toys', 'Snorkel gear', 'Beach blanket'],
  Outdoors: ['Hiking boots', 'Headlamp', 'Pocket knife', 'Hiking poles', 'Tent', 'Sleeping bag'],
  Kids: ['Kids’ clothes', 'Kids’ swimsuit', 'Toys', 'Books', 'Comfort toy or blanket', 'Noise machine'],
  Pets: ['Pet food', 'Bowls', 'Leash', 'Pet bed', 'Poop bags', 'Pet medicine', 'Vaccination records'],
  'Vacation rental': ['Toilet paper', 'Paper towels', 'Dish soap', 'Coffee', 'Kitchen knife', 'Cutting board', 'Board games'],
  Flying: ['Neck pillow', 'Sleep mask', 'Downloaded shows and podcasts', 'Pen for customs forms'],
  'Before you leave': ['Set the thermostat', 'Charge devices', 'Download boarding passes', 'Take out the trash', 'Hold the mail', 'Water the plants'],
  Work: ['Laptop bag', 'Badge', 'Mouse', 'Notebook and pen', 'Work clothes', 'Dress shoes'],
};
