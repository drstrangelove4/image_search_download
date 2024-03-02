using HtmlAgilityPack;

// This program searches for images from flickr and saves the images it finds.


//-----------------------------------------------------------------------------------------------------
// Create the link

// Base url for all queries 
const string BASE_URL = "https://flickr.com/search/?text=";

// Takes user input
(string full_url, string search_term) = Search(BASE_URL);

// Get the image links
List<string> image_links = new List<string>();
image_links =  Get_Image_Links(full_url);
// Throw an exception if there are no images count..
if (image_links.Count == 0)
{
    throw new Exception("No images have been found.");
}

// Download and save images
Get_Images(image_links, search_term);


//-----------------------------------------------------------------------------------------------------


(string, string) Search(string url_prefix)
    // Takes a URL prefix, adds the user search to it and returns the input. If nothing is entered keep promting user.
{
    string search_term = "";
    bool searching = true;
    while (searching)
    {   
        // Takes the user input and appends it to the url prefix passed to the function
        Console.WriteLine("What would you like to download images of?: ");
        search_term += Console.ReadLine();

        // In cases of entering nothing, keep prompting the user.
        if (search_term == "")
        {
            Console.WriteLine("Enter something!\n");
        }
        // If the user entered something then break the loop allowing for the return function to trigger.
        else
        {
            searching = false;      
        }
    }
    return (url_prefix + search_term, search_term);
}


//-----------------------------------------------------------------------------------------------------


List<string> Get_Image_Links (string url)
    // Get the image links from the html document
{
    // Create a web scraping object
    var web = new HtmlWeb();
    // Load the webpage
    var document = web.Load(url);
    // select the images from the HTML document
    var images = document.DocumentNode.QuerySelectorAll("img");

    // The list that will store our links 
    List<string> url_links = new List<string>();


    // Iterates over the image links found, adds the missing prefix and saves them to our results list. 
    foreach (var image in images)
    {

        string image_url = HtmlEntity.DeEntitize(image.Attributes["src"].Value);
        url_links.Add("http:" + image_url);

    }

    return url_links;
}

//-----------------------------------------------------------------------------------------------------

void Get_Images(List<string> image_links, string search_term)
    // Pulls each image from the provided link and saves the image.
{

    // Create a unique folder name for the images to be saved in
    string current_directory = Directory.GetCurrentDirectory();
    string current_date = DateTime.Now.ToString("dd-MM--HHmm-ss");
    string new_directory_name = $"{current_directory}\\{search_term}-{current_date}";
    // Create the new directory
    Directory.CreateDirectory(new_directory_name);

    // Give user information
    Console.WriteLine($"{image_links.Count} images have been found.");
    
    int image_number = 1;

    // Rip the image from the internet and save the file.
    foreach (string link in image_links)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.GetAsync(link).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                // Get the image as an array of bytes. 
                byte[] content = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                // Save the Byte to file 
                File.WriteAllBytes(new_directory_name + "\\" + image_number.ToString() + ".jpg", content);
                // Give user information
                Console.WriteLine($"Image {image_number} has been downloaded.");
            }
            else
            {
                Console.WriteLine($"Image at {link} could not be downloaded.");
            }
        }
        image_number++;
    }

    // Tell the user where the images are saved
    Console.WriteLine($"Images have been saved to:\n{new_directory_name}");
}