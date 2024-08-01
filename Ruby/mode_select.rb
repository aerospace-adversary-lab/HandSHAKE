
def parse_message(message)
  parts = message.split(',')
  message_type = parts[0]

  if parts.length > 1
    specific_id = parts[1]
  else
    specific_id = nil
  end
  
  # Return both the message type and specific ID
  return message_type, specific_id
end

def determine_mode(message_type, specific_id)
  MESSAGE_TYPES = {
    'M1' => {'mode' => 'Request', 'specific_id' => false},
    'M2' => {'mode' => 'Provider', 'specific_id' => true},
    'M3' => {'mode' => 'Request', 'specific_id' => true},
    'M4' => {'mode' => 'Provider', 'specific_id' => true},
    'M5b' => {'mode' => 'Consensus', 'specific_id' => false}
  }

  message_info = MESSAGE_TYPES[message_type]
  
  if message_info
    mode = message_info['mode']

    if message_info['specific_id'] && specific_id.nil?
      raise "Specific ID required for this message type."
    end
    return mode
  else
    raise "Unknown message type."
  end
end


def handle_connection(conn)
  loop do
    message = conn.gets
    
    if message.nil?
      break
    end

    message = message.chomp
    
    begin
      message_type, specific_id = parse_message(message)
      mode = determine_mode(message_type, specific_id)
      puts "Message: #{message} | Mode: #{mode}"
    rescue => e
      puts "Message: #{message} | Error: #{e}"
    end
  end
end


m1_stuff_1 = {
  "target_type" => "ground",
  "coordinates" => [5.0, 23.0],
  "time_window" => ["00:00", "00:00"],
  "requester_id" => "requester1",
  "proposer_id" => nil,
  "data" => nil,
  "requester_validation" => nil
}

m1_stuff_2 = {
  "target_type" => "orbital",
  "coordinates" => [1 00900U 64063C   24168.92942977  .00000843  00000+0  87068-3 0  9994
2 00900  90.2048  55.3767 0026184 347.2513 132.2215 13.75078085970997],
  "time_window" => ["2024-06-14 00:00", "2024-06-14 00:00"],
  "requester_id" => "requester456",
  "proposer_id" => nil,
  "data" => nil,
  "requester_validation" => nil
}

invalid_m1_stuff1 = {
  "coordinates" => [45.0, 13.0],
  "time_window" => ["2024-06-14 00:00", "2024-06-14 00:00"],
  "requester_id" => "requester123",
  "proposer_id" => nil,
  "data" => nil,
  "requester_validation" => nil
}

m2_stuff_1 = {
  "target_type" => "ground",
  "coordinates" => [5.0, 23.0],
  "time_window" => ["00:00", "00:00"],
  "requester_id" => "requester1",
  "proposer_id" => "proposer1",
  "data" => nil,
  "requester_validation" => nil
}

m2_stuff_2 = {
  "target_type" => "orbital",
  "coordinates" => [
    "1 00900U 64063C   24168.92942977  .00000843  00000+0  87068-3 0  9994",
    "2 00900  90.2048  55.3767 0026184 347.2513 132.2215 13.75078085970997"
  ],
  "time_window" => ["2024-06-14 00:00", "2024-06-14 00:00"],
  "requester_id" => "requester456",
  "proposer_id" => "proposer2",
  "data" => nil,
  "requester_validation" => nil
}

m3_stuff_1 = {
  "target_type" => "ground",
  "coordinates" => [5.0, 23.0],
  "time_window" => ["00:00", "00:00"],
  "requester_id" => "requester1",
  "proposer_id" => "proposer1",
  "data" => nil,
  "requester_validation" => nil
}

m3_stuff_2 = {
  "target_type" => "orbital",
  "coordinates" => [
    "1 00900U 64063C   24168.92942977  .00000843  00000+0  87068-3 0  9994",
    "2 00900  90.2048  55.3767 0026184 347.2513 132.2215 13.75078085970997"
  ],
  "time_window" => ["2024-06-14 00:00", "2024-06-14 00:00"],
  "requester_id" => "requester456",
  "proposer_id" => "proposer2",
  "data" => nil,
  "requester_validation" => nil
}

m4_stuff_1 = {
  "target_type" => "ground",
  "coordinates" => [5.0, 23.0],
  "time_window" => ["00:00", "00:00"],
  "requester_id" => "requester1",
  "proposer_id" => "proposer1",
  "data" => "some_data",
  "requester_validation" => nil
}

m4_stuff_2 = {
  "target_type" => "orbital",
  "coordinates" => [
    "1 00900U 64063C   24168.92942977  .00000843  00000+0  87068-3 0  9994",
    "2 00900  90.2048  55.3767 0026184 347.2513 132.2215 13.75078085970997"
  ],
  "time_window" => ["2024-06-14 00:00", "2024-06-14 00:00"],
  "requester_id" => "requester456",
  "proposer_id" => "proposer2",
  "data" => "some_orbital_data",
  "requester_validation" => nil
}

m5b_stuff_1 = {
  "target_type" => "ground",
  "coordinates" => [5.0, 23.0],
  "time_window" => ["00:00", "00:00"],
  "requester_id" => "requester1",
  "proposer_id" => "proposer1",
  "data" => "some_data",
  "requester_validation" => true
}

m6_stuff_1 = {
  "consensus_vote" => true
}

m6_stuff_2 = {
  "consensus_vote" => false
}